using Combinado.Api.Endpoints;
using Combinado.Api.Health;
using Combinado.Infrastructure;
using Combinado.Infrastructure.Logging;
using Combinado.Infrastructure.Messaging;
using Combinado.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = SerilogSetup.CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // DSN vazio = SDK desabilitado (dev sem Sentry, testes). Config em "Sentry" (appsettings / user-secrets / env).
    builder.WebHost.UseSentry(options =>
    {
        options.Environment = builder.Environment.EnvironmentName;
        options.TracesSampleRate = builder.Environment.IsProduction() ? 0.1 : 1.0;
        options.SendDefaultPii = false;
    });

    builder.Host.UseSerilog((context, services, configuration) =>
        SerilogSetup.Configure(configuration, context.Configuration, services, applicationName: "Combinado.Api"));

    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddRebusOneWayClient(
        DependencyInjection.RequireConnectionString(builder.Configuration, DependencyInjection.RabbitMqConnectionName));

    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapDevEndpoints();
    }

    app.MapHealthChecks("/health", new()
    {
        ResponseWriter = HealthResponseWriter.WriteAsync,
    });

    if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
    {
        await MigrateDatabaseAsync(app);
    }

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Combinado.Api encerrou inesperadamente");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Só para ambientes de dev/containers locais. Em staging/prod migrations rodam no pipeline (CLAUDE.md §10).
static async Task MigrateDatabaseAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<CombinadoDbContext>();
    Log.Information("Aplicando migrations pendentes (Database:MigrateOnStartup=true)");
    await db.Database.MigrateAsync();
}

/// <summary>
/// Exposto para <c>WebApplicationFactory&lt;Program&gt;</c> nos testes de integração.
/// </summary>
public partial class Program;
