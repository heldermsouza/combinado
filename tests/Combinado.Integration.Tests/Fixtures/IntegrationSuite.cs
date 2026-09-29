namespace Combinado.Integration.Tests.Fixtures;

/// <summary>
/// Todos os testes de integração compartilham a mesma <see cref="CombinadoApiFactory"/> (e os mesmos
/// containers). Testes da coleção rodam em série, o que é o esperado com banco compartilhado.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationSuite : ICollectionFixture<CombinadoApiFactory>
{
    public const string Name = "integration";
}
