using System.Net;
using System.Net.Http.Json;
using Combinado.Api.Health;
using Combinado.Integration.Tests.Fixtures;

namespace Combinado.Integration.Tests.Health;

public sealed class HealthEndpointTests(CombinadoApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GET_health_retorna_200_com_todas_as_dependencias_saudaveis()
    {
        var response = await Client.GetAsync(new Uri("/health", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<HealthResponseWriter.HealthResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
        Assert.Equal(["postgres", "rabbitmq", "redis"], body.Entries.Keys.Order());
        Assert.All(body.Entries.Values, entry => Assert.Equal("Healthy", entry.Status));
    }

    [Fact]
    public async Task GET_rota_desconhecida_em_api_v1_retorna_404()
    {
        var response = await Client.GetAsync(new Uri("/api/v1/nao-existe", UriKind.Relative), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Migrations_estao_aplicadas_no_banco_de_teste()
    {
        await using var connection = new Npgsql.NpgsqlConnection(Factory.PostgresConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = """select count(*) from "__EFMigrationsHistory";""";
        var applied = (long)(await command.ExecuteScalarAsync(TestContext.Current.CancellationToken))!;

        Assert.True(applied >= 1, "Pelo menos a migration InitialCreate deve estar aplicada.");
    }
}
