using Combinado.Infrastructure.Health;
using Combinado.Infrastructure.Messaging;
using Combinado.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Combinado.Infrastructure;

public static class DependencyInjection
{
    public const string PostgresConnectionName = "Postgres";
    public const string RedisConnectionName = "Redis";
    public const string RabbitMqConnectionName = "RabbitMq";

    /// <summary>
    /// Persistência, cache e health checks. Não registra o bus: cada host decide se é
    /// one-way client (<see cref="RebusConfiguration.AddRebusOneWayClient"/>) ou worker
    /// (<see cref="RebusConfiguration.AddRebusWorker"/>).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var postgres = RequireConnectionString(configuration, PostgresConnectionName);
        var redis = RequireConnectionString(configuration, RedisConnectionName);
        var rabbitMq = RequireConnectionString(configuration, RabbitMqConnectionName);

        services.AddDbContext<CombinadoDbContext>(options => PostgresOptions.Configure(options, postgres));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redis);
            options.AbortOnConnectFail = false; // app sobe mesmo com Redis fora; health check reporta
            options.ClientName = "combinado";
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddHealthChecks()
            .AddDbContextCheck<CombinadoDbContext>("postgres", tags: ["ready"])
            .AddCheck<RedisHealthCheck>("redis", tags: ["ready"])
            .AddCheck("rabbitmq", new RabbitMqHealthCheck(rabbitMq), tags: ["ready"]);

        return services;
    }

    public static string RequireConnectionString(IConfiguration configuration, string name)
    {
        return configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"ConnectionStrings:{name} não configurada.");
    }
}
