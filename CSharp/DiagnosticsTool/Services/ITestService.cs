using DiagnosticsTool.Models.Tests;

namespace DiagnosticsTool.Services;

public interface ITestService
{
    IReadOnlyList<DiagnosticsToolTest> GetTests();
    DiagnosticsToolTest? GetTest(string testId);
    Task<DiagnosticsToolTestExecutionResult> ExecuteTestAsync(string testId, Uri baseUri, CancellationToken cancellationToken = default);
}

