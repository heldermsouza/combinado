using Serilog;
using Serilog.Events;

namespace Combinado.Api.Logging;

/// <summary>
/// Configuração Serilog compartilhada pelos hosts. Console sempre; Seq quando <c>Seq:ServerUrl</c> existir.
/// Níveis mínimos vêm de <c>Serilog:MinimumLevel</c> no appsettings.
/// </summary>
public static class SerilogSetup
{
    public static Serilog.ILogger CreateBootstrapLogger() =>
        new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateBootstrapLogger();

    public static void Configure(
        LoggerConfiguration logger,
        IConfiguration configuration,
        IServiceProvider services,
        string applicationName)
    {
        logger
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("Application", applicationName)
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information);

        var seqUrl = configuration["Seq:ServerUrl"];
        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            logger.WriteTo.Seq(seqUrl, apiKey: configuration["Seq:ApiKey"]);
        }
    }
}
