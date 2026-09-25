using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Retry.Simple;

namespace Combinado.Infrastructure.Messaging;

/// <summary>
/// Configuração do Rebus sobre RabbitMQ (ADR-0004). Filas e política de retry são as mesmas
/// para todos os hosts; o que muda é se o host consome (tem fila de entrada) ou só publica.
/// </summary>
public static class RebusConfiguration
{
    public const string ErrorQueue = "combinado.error";
    public const string WhatsAppIncomingQueue = "wa.incoming";
    public const string WhatsAppOutboundQueue = "wa.outbound";

    private const int MaxDeliveryAttempts = 5;

    /// <summary>
    /// Host que só publica/envia (Api). Não cria fila de entrada nem workers.
    /// </summary>
    public static IServiceCollection AddRebusOneWayClient(this IServiceCollection services, string rabbitMqConnectionString)
    {
        return services.AddRebus(configure => configure
            .Logging(l => l.Serilog())
            .Transport(t => t.UseRabbitMqAsOneWayClient(rabbitMqConnectionString)));
    }

    /// <summary>
    /// Host que consome de <paramref name="inputQueue"/> (workers). Mensagens que falham
    /// <see cref="MaxDeliveryAttempts"/> vezes vão para <see cref="ErrorQueue"/>.
    /// </summary>
    public static IServiceCollection AddRebusWorker(
        this IServiceCollection services,
        string rabbitMqConnectionString,
        string inputQueue,
        int maxParallelism = 5)
    {
        return services.AddRebus(configure => configure
            .Logging(l => l.Serilog())
            .Transport(t => t.UseRabbitMq(rabbitMqConnectionString, inputQueue))
            .Options(o =>
            {
                o.RetryStrategy(errorQueueName: ErrorQueue, maxDeliveryAttempts: MaxDeliveryAttempts);
                o.SetNumberOfWorkers(1);
                o.SetMaxParallelism(maxParallelism);
            }));
    }
}
