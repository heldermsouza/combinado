using Combinado.Infrastructure;
using Combinado.Infrastructure.Logging;
using Combinado.Infrastructure.Messaging;
using Combinado.SchedulerWorker;
using Serilog;

Log.Logger = SerilogSetup.CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Logging.AddSentry(options =>
    {
        options.Dsn = builder.Configuration["Sentry:Dsn"] ?? string.Empty;
        options.Environment = builder.Environment.EnvironmentName;
        options.SendDefaultPii = false;
    });

    builder.Services.AddSerilog((services, configuration) =>
        SerilogSetup.Configure(configuration, builder.Configuration, services, applicationName: "Combinado.SchedulerWorker"));

    builder.Services.AddInfrastructure(builder.Configuration);

    // Só publica eventos (ex.: ResumoSemanalPronto); não consome fila.
    builder.Services.AddRebusOneWayClient(
        DependencyInjection.RequireConnectionString(builder.Configuration, DependencyInjection.RabbitMqConnectionName));

    builder.Services.AddHostedService<Heartbeat>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Combinado.SchedulerWorker encerrou inesperadamente");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
