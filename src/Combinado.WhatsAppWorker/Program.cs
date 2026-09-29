using Combinado.Infrastructure;
using Combinado.Infrastructure.Logging;
using Combinado.Infrastructure.Messaging;
using Combinado.WhatsAppWorker;
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
        SerilogSetup.Configure(configuration, builder.Configuration, services, applicationName: "Combinado.WhatsAppWorker"));

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddRebusWorker(
        DependencyInjection.RequireConnectionString(builder.Configuration, DependencyInjection.RabbitMqConnectionName),
        inputQueue: RebusConfiguration.WhatsAppIncomingQueue);

    builder.Services.AddHostedService<Heartbeat>();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Combinado.WhatsAppWorker encerrou inesperadamente");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
