using Combinado.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace Combinado.Integration.Tests.Fixtures;

/// <summary>
/// Sobe Postgres, Redis e RabbitMQ reais (imagens iguais ao docker-compose) uma vez por assembly,
/// aponta a Api para eles e aplica as migrations. <see cref="ResetDatabaseAsync"/> limpa as tabelas
/// entre testes via Respawn, preservando o histórico de migrations.
/// </summary>
public sealed class CombinadoApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Mesmas tags do infra/docker-compose.yml: o que passa aqui é o que roda em dev.
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("combinado")
        .WithUsername("combinado")
        .WithPassword("combinado")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3.13-alpine")
        .WithUsername("combinado")
        .WithPassword("combinado")
        .Build();

    private Respawner? _respawner;

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync(), _rabbitMq.StartAsync());

        await using var scope = Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CombinadoDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(PostgresConnectionString);
        await connection.OpenAsync();

        // Respawn lança se não houver nenhuma tabela além das ignoradas. Enquanto o modelo está vazio
        // (Fase 0), não há nada a limpar; a partir da primeira entidade o reset passa a rodar de verdade.
        if (!await HasResettableTablesAsync(connection))
        {
            return;
        }

        _respawner ??= await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [CombinadoDbContext.DefaultSchema],
            TablesToIgnore = [new Table("__EFMigrationsHistory")],
        });

        await _respawner.ResetAsync(connection);
    }

    private static async Task<bool> HasResettableTablesAsync(NpgsqlConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            select count(*)
            from information_schema.tables
            where table_schema = @schema
              and table_type = 'BASE TABLE'
              and table_name <> '__EFMigrationsHistory'
            """;
        command.Parameters.AddWithValue("schema", CombinadoDbContext.DefaultSchema);

        return (long)(await command.ExecuteScalarAsync())! > 0;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Postgres", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", _redis.GetConnectionString());
        builder.UseSetting("ConnectionStrings:RabbitMq", _rabbitMq.GetConnectionString());
        builder.UseSetting("Seq:ServerUrl", string.Empty);
        builder.UseSetting("Database:MigrateOnStartup", "false");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _redis.DisposeAsync().AsTask(),
            _rabbitMq.DisposeAsync().AsTask());
    }
}
