using Combinado.Infrastructure.Messaging;

namespace Combinado.WhatsAppWorker;

/// <summary>
/// Sinal de vida do worker enquanto não há handlers Rebus (Fase 2). Fica depois como
/// base de métrica de disponibilidade do processo.
/// </summary>
public sealed partial class Heartbeat(ILogger<Heartbeat> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(logger, RebusConfiguration.WhatsAppIncomingQueue);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            LogHeartbeat(logger);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "WhatsAppWorker iniciado; ouvindo fila {Queue}")]
    private static partial void LogStarted(ILogger logger, string queue);

    [LoggerMessage(Level = LogLevel.Debug, Message = "WhatsAppWorker heartbeat")]
    private static partial void LogHeartbeat(ILogger logger);
}
