using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenTraceability.Queries.Diagnostics.Rules;

/// <summary>
/// This rule validates that the response from the Master Data Resolver is a valid HTTP response
/// with a success status code. If the response is not a success status code, then the rule will
/// return a validation result with a type of "HttpError" and a log level of "Error".
/// </summary>
public class MasterDataHttpResponseRule : IDiagnosticsRequestRule
{
    public string Key { get; set; } = "OT_DIAG_RULE_MD_HTTP_RESPONSE";

    public Task<List<DiagnosticsValidationResult>> ExecuteAsync(params object[] obj)
    {
        var results = new List<DiagnosticsValidationResult>();

        if (obj.Length < 1)
        {
            throw new ArgumentException("HttpResponseMessage parameter is required.");
        }

        var response = obj[0] as System.Net.Http.HttpResponseMessage;
        if (response == null)
        {
            throw new ArgumentException("HttpResponseMessage parameter is null or invalid.");
        }

        // Validate HTTP status code
        if (!response.IsSuccessStatusCode)
        {
            results.Add(new DiagnosticsValidationResult
            {
                Level = LogLevel.Error,
                Type = DiagnosticsValidationType.HttpError,
                RuleKey = Key,
                Message = $"Master Data Resolver returned non-success status code: {(int)response.StatusCode} {response.StatusCode}"
            });
        }

        return Task.FromResult(results);
    }
}