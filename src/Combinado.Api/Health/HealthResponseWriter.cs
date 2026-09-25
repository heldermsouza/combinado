using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Combinado.Api.Health;

/// <summary>
/// Serializa o <see cref="HealthReport"/> como JSON legível por humanos e pelo Uptime Robot,
/// no lugar do texto puro "Healthy" padrão.
/// </summary>
public static class HealthResponseWriter
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new HealthResponse(
            Status: report.Status.ToString(),
            TotalDurationMs: Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            Entries: report.Entries.ToDictionary(
                e => e.Key,
                e => new HealthEntry(
                    Status: e.Value.Status.ToString(),
                    DurationMs: Math.Round(e.Value.Duration.TotalMilliseconds, 1),
                    Description: e.Value.Description,
                    Error: e.Value.Exception?.Message)));

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, Options), context.RequestAborted);
    }

    public sealed record HealthResponse(string Status, double TotalDurationMs, Dictionary<string, HealthEntry> Entries);

    public sealed record HealthEntry(string Status, double DurationMs, string? Description, string? Error);
}
