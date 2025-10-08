using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenTraceability.Queries.Diagnostics;

/// <summary>
/// The type of request that was performed when querying for traceability data.
/// </summary>
public enum RequestType
{
    /// <summary>
    /// The request was performed against a Digital Link.
    /// </summary>  
    DigitalLink,

    /// <summary>
    /// The request was performed against an EPCIS Query Interface.
    /// </summary>
    EPCIS,

    /// <summary>
    /// The request was performed against a Master Data Resolver.
    /// </summary>
    MasterData
}

public class DiagnosticsRequest
{
    /// <summary>
    /// The options that were used to perform the request.
    /// </summary>
    public object? RequestOptions { get; set; } = null;

    /// <summary>
    /// The list of validations that were performed against the singular request.
    /// </summary>
    public List<DiagnosticsValidationResult> Validations { get; set; } = new List<DiagnosticsValidationResult>();

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
