using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Combinado.Infrastructure.Health;

/// <summary>
/// Abre uma conexão AMQP curta e verifica que o broker responde. Não reaproveita a conexão do Rebus
/// de propósito: o probe deve refletir o broker, não o estado interno do bus.
/// </summary>
public sealed class RabbitMqHealthCheck(string connectionString) : IHealthCheck
{
    private readonly ConnectionFactory _factory = new()
    {
        Uri = new Uri(connectionString),
        ClientProvidedName = "combinado-healthcheck",
    };

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _factory.CreateConnectionAsync(cancellationToken);
            return connection.IsOpen
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Conexão AMQP não abriu");
        }
        catch (BrokerUnreachableException ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ indisponível", ex);
        }
        catch (OperationInterruptedException ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ recusou a conexão", ex);
        }
    }
}
