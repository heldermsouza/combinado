using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Combinado.Infrastructure.Logging;

/// <summary>
/// Configuração Serilog compartilhada por Api e workers. Console sempre; Seq quando <c>Seq:ServerUrl</c>
/// existir; Sentry (eventos a partir de Error, breadcrumbs a partir de Information) quando <c>Sentry:Dsn</c> existir.
/// Níveis mínimos vêm de <c>Serilog:MinimumLevel</c> no appsettings.
/// </summary>
public static class SerilogSetup
{
    public static ILogger CreateBootstrapLogger() =>
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

        if (!string.IsNullOrWhiteSpace(configuration["Sentry:Dsn"]))
        {
            // O SDK já foi inicializado pelo host (UseSentry / AddSentry); aqui só encaminhamos logs.
            logger.WriteTo.Sentry(options =>
            {
                options.InitializeSdk = false;
                options.MinimumBreadcrumbLevel = LogEventLevel.Information;
                options.MinimumEventLevel = LogEventLevel.Error;
            });
        }
    }
}
