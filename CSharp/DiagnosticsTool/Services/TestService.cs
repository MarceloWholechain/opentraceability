using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DiagnosticsTool.Models.Tests;

namespace DiagnosticsTool.Services;

public sealed class TestService : ITestService
{
    private readonly Lazy<IReadOnlyDictionary<string, DiagnosticsToolTest>> _tests;
    private readonly string _testsRoot;
    private readonly IHttpClientFactory _httpClientFactory;

    public TestService(IWebHostEnvironment environment, IHttpClientFactory httpClientFactory)
    {
        _testsRoot = Path.Combine(environment.ContentRootPath, "Tests");
        _httpClientFactory = httpClientFactory;
        _tests = new Lazy<IReadOnlyDictionary<string, DiagnosticsToolTest>>(LoadTests, true);
    }

    public IReadOnlyList<DiagnosticsToolTest> GetTests()
        => _tests.Value.Values.ToList();

    public DiagnosticsToolTest? GetTest(string testId)
    {
        if (string.IsNullOrWhiteSpace(testId))
        {
            return null;
        }

        return _tests.Value.TryGetValue(testId, out var test) ? test : null;
    }

    public async Task<DiagnosticsToolTestExecutionResult> ExecuteTestAsync(string testId, Uri baseUri, CancellationToken cancellationToken = default)
    {
        var test = GetTest(testId) ?? throw new InvalidOperationException($"Test '{testId}' was not found.");
        var config = test.Config;

        if (string.IsNullOrWhiteSpace(config.Endpoint))
        {
            throw new InvalidOperationException($"Test '{testId}' is missing an endpoint.");
        }

        if (!Uri.TryCreate(config.Endpoint, UriKind.RelativeOrAbsolute, out var endpointUri))
        {
            throw new InvalidOperationException($"Test '{testId}' has an invalid endpoint.");
        }

        if (!endpointUri.IsAbsoluteUri)
        {
            endpointUri = new Uri(baseUri, config.Endpoint.TrimStart('/'));
        }

        var client = _httpClientFactory.CreateClient("default");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpointUri);

        if (config.Body.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
        {
            var json = config.Body.GetRawText();
            json = ReplaceTestPlaceholder(json, baseUri);
            requestMessage.Content = new StringContent(json, Encoding.UTF8);
            requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        var response = await client.SendAsync(requestMessage, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Test '{testId}' failed with status {(int)response.StatusCode}: {body}");
        }

        string? diagnosticsId = null;
        if (response.Headers.TryGetValues("X-Diagnostics-Id", out var values))
        {
            diagnosticsId = values.FirstOrDefault();
        }

        return new DiagnosticsToolTestExecutionResult(diagnosticsId);
    }

    private static string ReplaceTestPlaceholder(string json, Uri baseUri)
    {
        const string placeholder = "{TEST_LOCAL_URL}";

        if (string.IsNullOrEmpty(json) || !json.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
        {
            return json;
        }

        var host = baseUri.IsDefaultPort
            ? $"{baseUri.Scheme}://{baseUri.Host}"
            : $"{baseUri.Scheme}://{baseUri.Host}:{baseUri.Port}";

        return json.Replace(placeholder, host, StringComparison.OrdinalIgnoreCase);
    }

    private IReadOnlyDictionary<string, DiagnosticsToolTest> LoadTests()
    {
        if (!Directory.Exists(_testsRoot))
        {
            return new Dictionary<string, DiagnosticsToolTest>(StringComparer.OrdinalIgnoreCase);
        }

        var tests = new Dictionary<string, DiagnosticsToolTest>(StringComparer.OrdinalIgnoreCase);

        foreach (var testDir in Directory.EnumerateDirectories(_testsRoot))
        {
            var testId = Path.GetFileName(testDir)!;
            try
            {
                var configPath = Path.Combine(testDir, "test-config.json");
                if (!File.Exists(configPath))
                {
                    continue;
                }

                var configJson = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<DiagnosticsToolTestConfig>(configJson, JsonOptions())!;

                var requests = LoadRequests(testDir).ToList();
                if (requests.Count == 0)
                {
                    continue;
                }

                tests[testId] = new DiagnosticsToolTest(testId, config, requests);
            }
            catch
            {
                // swallow individual test errors to avoid crashing the service
            }
        }

        return tests;
    }

    private IEnumerable<DiagnosticsToolTestRequest> LoadRequests(string testDir)
    {
        foreach (var requestDir in Directory.EnumerateDirectories(testDir))
        {
            var requestId = Path.GetFileName(requestDir)!;

            var requestPath = Path.Combine(requestDir, "request.json");
            var responsePath = Path.Combine(requestDir, "response.json");

            if (!File.Exists(requestPath) || !File.Exists(responsePath))
            {
                continue;
            }

            var requestJson = File.ReadAllText(requestPath);
            var request = JsonSerializer.Deserialize<DiagnosticsToolTestRequestDetails>(requestJson, JsonOptions());
            if (request == null)
            {
                continue;
            }

            var responseJson = File.ReadAllText(responsePath);
            var responseModel = JsonSerializer.Deserialize<DiagnosticsToolTestResponseFile>(responseJson, JsonOptions());
            if (responseModel == null)
            {
                continue;
            }

            var bodyFile = Directory.EnumerateFiles(requestDir, "response-body.*", SearchOption.TopDirectoryOnly).FirstOrDefault();
            byte[] bodyBytes = bodyFile != null ? File.ReadAllBytes(bodyFile) : Array.Empty<byte>();

            var response = new DiagnosticsToolTestResponse(
                responseModel.StatusCode ?? StatusCodes.Status200OK,
                responseModel.Headers?.GetValueOrDefault("Content-Type"),
                responseModel.Headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                bodyBytes);

            yield return new DiagnosticsToolTestRequest(requestId, request, response);
        }
    }

    private static JsonSerializerOptions JsonOptions()
        => new()
        {
            PropertyNameCaseInsensitive = true
        };

    private sealed class DiagnosticsToolTestResponseFile
    {
        public int? StatusCode { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }
}

