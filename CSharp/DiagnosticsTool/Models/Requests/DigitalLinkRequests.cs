using OpenTraceability.Models.Identifiers;
using OpenTraceability.Queries;

namespace DiagnosticsTool.Models.Requests;

public class DigitalLinkEpcRequest
{
    public DigitalLinkQueryOptions Options { get; set; } = new DigitalLinkQueryOptions();
    public string EPC { get; set; } = string.Empty;
}

public class DigitalLinkPglnRequest
{
    public DigitalLinkQueryOptions Options { get; set; } = new DigitalLinkQueryOptions();
    public string PGLN { get; set; } = string.Empty;
}
