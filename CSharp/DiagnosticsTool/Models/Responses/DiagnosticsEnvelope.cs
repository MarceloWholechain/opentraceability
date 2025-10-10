using OpenTraceability.Queries;
using OpenTraceability.Queries.Diagnostics;

namespace DiagnosticsTool.Models.Responses;

public interface IDiagnosticsEnvelope
{
    DiagnosticsReport Diagnostics { get; }
    bool HasSchemaErrors { get; }
    object? DataObject { get; }
}

public class DiagnosticsEnvelope<T> : IDiagnosticsEnvelope
{
    public T? Data { get; set; }
    public DiagnosticsReport Diagnostics { get; set; } = new DiagnosticsReport();
    public bool HasSchemaErrors => Diagnostics.HasSchemaErrors;

    // explicit interface helpers
    DiagnosticsReport IDiagnosticsEnvelope.Diagnostics => Diagnostics;
    bool IDiagnosticsEnvelope.HasSchemaErrors => HasSchemaErrors;
    object? IDiagnosticsEnvelope.DataObject => Data;
}

public class ResolvedUrlResult
{
    public Uri? EpcisQueryInterfaceUrl { get; set; }
}
