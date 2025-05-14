using System.Diagnostics;

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

        // Получаем или генерируем CorrelationId
        string correlationId;
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationIdValues))
        {
            correlationId = Activity.Current?.Id ?? Guid.NewGuid().ToString();
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }
        else
        {
            correlationId = correlationIdValues.ToString();
        }

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var method = context.Request.Method;
        var path = context.Request.Path;
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        var clientId = "Unknown";

        using (_logger.BeginScope(new Dictionary<string, object> { 
            { "CorrelationId", correlationId }, 
            { "ClientIp", correlationId }, 
            { "HttpMethod", method } }))
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
                _logger.LogError(ex,
                    "Unhandled exception processing request from {ClientIp} (ClientId: {ClientId}) - {Method} {Path} in {Elapsed:0.0000} ms",
                    clientIp, clientId, method, path, stopwatch.Elapsed.TotalMilliseconds);
                throw;
            }
        }
    }
}