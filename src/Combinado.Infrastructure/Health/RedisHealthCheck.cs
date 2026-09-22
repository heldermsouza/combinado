using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Combinado.Infrastructure.Health;

/// <summary>
/// PING no Redis via o multiplexer compartilhado da aplicação.
/// </summary>
public sealed class RedisHealthCheck(IConnectionMultiplexer redis) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var latency = await redis.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy($"PING {latency.TotalMilliseconds:F1} ms");
        }
        catch (RedisException ex)
        {
            return HealthCheckResult.Unhealthy("Redis indisponível", ex);
        }
    }
}
