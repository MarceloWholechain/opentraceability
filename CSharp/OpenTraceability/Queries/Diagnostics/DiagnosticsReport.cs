using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenTraceability.Queries.Diagnostics;

/// <summary>
/// The report of the diagnostics.
/// </summary>
public class DiagnosticsReport
{
    /// <summary>
    /// Whether the report has any schema errors anywhere in the report.
    /// </summary>
    public bool HasSchemaErrors
    {
        get
        {
            return Validations.Any(v => v.Type == DiagnosticsValidationType.SchemaError)
                || Requests.Any(r => r.Validations.Any(v => v.Type == DiagnosticsValidationType.SchemaError));
        }
    }

    /// <summary>
    /// One or more requests that were performed in chronological order.
    /// </summary>
    public List<DiagnosticsRequest> Requests { get; set; } = new List<DiagnosticsRequest>();

    /// <summary>
    /// The list of validations that were performed.
    /// </summary>
    public List<DiagnosticsValidationResult> Validations { get; set; } = new List<DiagnosticsValidationResult>();

    /// <summary>
    /// Creates a new request.
    /// </summary>
    public void NewRequest(object? requestOptions = null)
    {
        Requests.Add(new DiagnosticsRequest()
        {
            RequestOptions = requestOptions
        });
    }

    /// <summary>
    /// The current request that is being processed.
    /// </summary>
    public DiagnosticsRequest CurrentRequest
    {
        get
        {
            if (Requests.Count == 0)
            {
                throw new Exception("No requests have been made yet.");
            }

            return Requests.Last();
        }
    }

    /// <summary>
    /// Executes a rule against the request.
    /// </summary>
    /// <typeparam name="T">The type of the rule to execute.</typeparam>
    /// <param name="parameters">The parameters to pass to the rule.</param>
    /// <exception cref="Exception"></exception>
    public async Task ExecuteRuleAsync<T>(params object[] parameters) where T : IDiagnosticsRequestRule
    {
        // Construct the rule
        var rule = Activator.CreateInstance(typeof(T)) as IDiagnosticsRequestRule;
        if (rule == null)
        {
            throw new Exception($"Rule {typeof(T).Name} not found.");
        }

        // Execute the rule
        var results = await rule.ExecuteAsync(parameters);
        Validations.AddRange(results);
    }
}