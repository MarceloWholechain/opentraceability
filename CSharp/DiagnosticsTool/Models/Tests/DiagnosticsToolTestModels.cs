using System.Text.Json;

namespace DiagnosticsTool.Models.Tests;

public record DiagnosticsToolTest(string TestId, DiagnosticsToolTestConfig Config, IReadOnlyList<DiagnosticsToolTestRequest> Requests)
{
    public DiagnosticsToolTestRequest? FindRequest(string url)
        => Requests.FirstOrDefault(r => string.Equals(r.Request.Url, Normalize(url), StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return "/";
        }

        return url.StartsWith("/") ? url : "/" + url;
    }
}

public record DiagnosticsToolTestSummary(string TestId, string? Name, string? Description);

public record DiagnosticsToolTestConfig(string? Name, string? Description, string Endpoint, JsonElement Body);

public record DiagnosticsToolTestRequest(string RequestId, DiagnosticsToolTestRequestDetails Request, DiagnosticsToolTestResponse Response);

public record DiagnosticsToolTestRequestDetails(string Url);

public record DiagnosticsToolTestResponse(int StatusCode, string? ContentType, IReadOnlyDictionary<string, string>? Headers, byte[] BodyBytes)
{
    public string? BodyText => BodyBytes.Length == 0 ? null : System.Text.Encoding.UTF8.GetString(BodyBytes);
}

public record DiagnosticsToolTestExecutionResult(string? DiagnosticsId);

