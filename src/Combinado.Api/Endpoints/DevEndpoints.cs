namespace Combinado.Api.Endpoints;

/// <summary>
/// Endpoints mapeados <b>apenas</b> em Development (ver Program.cs). Servem para validar a
/// observabilidade ponta a ponta sem depender de um bug real.
/// </summary>
public static class DevEndpoints
{
    public static IEndpointRouteBuilder MapDevEndpoints(this IEndpointRouteBuilder app)
    {
        var dev = app.MapGroup("/api/v1/dev").WithTags("dev");

        dev.MapGet("/sentry-test", () =>
        {
            throw new InvalidOperationException("Exceção proposital para validar o Sentry (GET /api/v1/dev/sentry-test).");
        })
        .WithSummary("Lança uma exceção não tratada para verificar captura no Sentry. Só em Development.");

        return app;
    }
}
