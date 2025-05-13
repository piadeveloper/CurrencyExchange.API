using CurrencyExchange.API.Middleware;
using CurrencyExchange.API.Providers;
using CurrencyExchange.API.Providers.Interfaces;
using CurrencyExchange.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Serilog;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CurrencyExchange.API.Helpers
{
    public static class AppBuilderExtensions
    {
        public static IServiceCollection AddCurrencyServices(this IServiceCollection services)
        {
            services.AddTransient<ICurrencyConverterProviderFactory, CurrencyConverterProviderFactory>();

            services.AddHttpClient<FrankfurterProvider>(client =>
            {
                client.BaseAddress = new Uri("https://api.frankfurter.dev/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(response => !response.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, attempt =>
                {
                    double jitter = new Random().NextDouble() * 0.5;
                    return TimeSpan.FromSeconds(Math.Pow(2, attempt) + jitter);
                }))
            .AddPolicyHandler(Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(response => !response.IsSuccessStatusCode)
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))
            );

            return services;
        }

        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }

        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
        
        public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
        {
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            builder.Host.UseSerilog((context, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .WriteTo.Console();
            });

            return builder;
        }
        public static WebApplicationBuilder AddOpenTelemetryTracing(this WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration;

            var otelEndpoint = configuration.GetValue<string>("OpenTelemetry:Endpoint");
            var serviceName = configuration.GetValue<string>("OpenTelemetry:Resource:service.name");
            var source = configuration.GetValue<string>("OpenTelemetry:Source");


            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing.AddAspNetCoreInstrumentation();
                    tracing.AddHttpClientInstrumentation();
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint); 
                    });
                })
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation();
                    metrics.AddHttpClientInstrumentation();
                    metrics.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otelEndpoint);
                    });
                });

            return builder;
        }

        public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Currency Exchange API",
                    Description = "API for currency exchange"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter your JWT token with Bearer prefix (e.g. Bearer {token})",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return builder;
        }

        public static WebApplicationBuilder AddJwtAuth(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<JwtTokenService>();
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
                        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                        ClockSkew = TimeSpan.Zero
                    };
                    options.Events = new JwtBearerEvents
                    {
                        // this code is for debug only. It should be deleted for prod
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine("Authentication failed: " + context.Exception.Message);

                            Console.WriteLine("Request Headers:");
                            foreach (var header in context.Request.Headers)
                            {
                                Console.WriteLine($"{header.Key}: {header.Value}");
                            }

                            if (context.Request.Headers.ContainsKey("Authorization"))
                            {
                                try
                                {
                                    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                                    var handler = new JwtSecurityTokenHandler();
                                    var jwtToken = handler.ReadJwtToken(token);

                                    Console.WriteLine("\nDecoded JWT:");
                                    Console.WriteLine($"Issuer: {jwtToken.Issuer}");
                                    Console.WriteLine($"Audience: {string.Join(", ", jwtToken.Audiences)}");
                                    Console.WriteLine($"Subject: {jwtToken.Subject}");
                                    Console.WriteLine($"Claims:");
                                    foreach (var claim in jwtToken.Claims)
                                    {
                                        Console.WriteLine($" - {claim.Type}: {claim.Value}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Failed to decode token: {ex.Message}");
                                }
                            }

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("Token validated successfully");

                            // Логируем заголовки
                            Console.WriteLine("Request Headers:");
                            foreach (var header in context.Request.Headers)
                            {
                                Console.WriteLine($"{header.Key}: {header.Value}");
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return builder;
        }
    }
}
