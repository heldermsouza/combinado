namespace Combinado.Integration.Tests.Fixtures;

/// <summary>
/// Base para testes HTTP: cliente pronto e banco limpo antes de cada teste.
/// </summary>
[Collection(IntegrationSuite.Name)]
public abstract class IntegrationTestBase(CombinadoApiFactory factory) : IAsyncLifetime
{
    protected CombinadoApiFactory Factory { get; } = factory;

    protected HttpClient Client { get; } = factory.CreateClient();

    public virtual async ValueTask InitializeAsync() => await Factory.ResetDatabaseAsync();

    public virtual ValueTask DisposeAsync()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
