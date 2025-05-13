using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var method = context.Request.Method;
        var path = context.Request.Path;
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        var clientId = "Unknown";
        using (_logger.BeginScope(new Dictionary<string, object> { { "CorrelationId", correlationId } }))
        {
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);
                    clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "Unknown";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to parse JWT token: {Message}", ex.Message);
                }
            }

            try
            {
                _logger.LogInformation("Request started with CorrelationId: {CorrelationId}", correlationId);
                await _next(context);
                stopwatch.Stop();

                _logger.LogInformation(
                "Request from {ClientIp} (ClientId: {ClientId}) - {Method} {Path} responded {StatusCode} in {Elapsed:0.0000} ms",
                clientIp,
                clientId,
                method,
                path,
                context.Response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds
                );


                _logger.LogInformation("Request finished with CorrelationId: {CorrelationId}", correlationId);

            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error(ex, "Unhandled exception processing request from {ClientIp} (ClientId: {ClientId}) - {Method} {Path} in {Elapsed:0.0000} ms",
                    clientIp, clientId, method, path, stopwatch.Elapsed.TotalMilliseconds);
                throw;
            }
        }
        
    }
}