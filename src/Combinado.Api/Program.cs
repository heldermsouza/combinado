var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

await app.RunAsync();

/// <summary>
/// Exposto para <c>WebApplicationFactory&lt;Program&gt;</c> nos testes de integração.
/// </summary>
public partial class Program;
