namespace Combinado.SchedulerWorker;

/// <summary>
/// Sinal de vida do scheduler enquanto não há jobs agendados (Fase 3: RF07, D-3/D-1, reset mensal).
/// </summary>
public sealed partial class Heartbeat(ILogger<Heartbeat> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogStarted(logger);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            LogHeartbeat(logger);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "SchedulerWorker iniciado; nenhum job agendado nesta fase")]
    private static partial void LogStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "SchedulerWorker heartbeat")]
    private static partial void LogHeartbeat(ILogger logger);
}
