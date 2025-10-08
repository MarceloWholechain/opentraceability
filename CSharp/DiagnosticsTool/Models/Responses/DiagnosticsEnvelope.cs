using OpenTraceability.Queries;
using OpenTraceability.Queries.Diagnostics;

namespace DiagnosticsTool.Models.Responses;

public class DiagnosticsEnvelope<T>
{
    public T? Data { get; set; }
    public DiagnosticsReport Diagnostics { get; set; } = new DiagnosticsReport();
    public bool HasSchemaErrors => Diagnostics.HasSchemaErrors;
}

public class ResolvedUrlResult
{
    public Uri? EpcisQueryInterfaceUrl { get; set; }
}
