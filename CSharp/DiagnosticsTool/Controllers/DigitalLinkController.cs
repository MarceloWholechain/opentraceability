using DiagnosticsTool.Models.Requests;
using DiagnosticsTool.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using OpenTraceability.Models.Identifiers;
using OpenTraceability.Queries;
using OpenTraceability.Queries.Diagnostics;

namespace DiagnosticsTool.Controllers;

[ApiController]
[Route("api/digitallink")] 
public class DigitalLinkController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DigitalLinkController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("epcis-url/epc")] 
    public async Task<ActionResult<DiagnosticsEnvelope<ResolvedUrlResult>>> ResolveEpcUrl([FromBody] DigitalLinkEpcRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.EPC))
        {
            return BadRequest("EPC is required.");
        }
        if (request.Options?.URL == null)
        {
            return BadRequest("Options.URL is required.");
        }
        if (!OpenTraceability.Models.Identifiers.EPC.TryParse(request.EPC, out var epc, out var epcErr))
        {
            return BadRequest(epcErr);
        }

        var report = new DiagnosticsReport();
        var client = _httpClientFactory.CreateClient("default");
        var url = await EPCISTraceabilityResolver.GetEPCISQueryInterfaceURL(request.Options, epc, client, report);
        var envelope = new DiagnosticsEnvelope<ResolvedUrlResult>
        {
            Data = new ResolvedUrlResult { EpcisQueryInterfaceUrl = url },
            Diagnostics = report
        };
        return Ok(envelope);
    }

    [HttpPost("epcis-url/pgln")] 
    public async Task<ActionResult<DiagnosticsEnvelope<ResolvedUrlResult>>> ResolvePglnUrl([FromBody] DigitalLinkPglnRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.PGLN))
        {
            return BadRequest("PGLN is required.");
        }
        if (request.Options?.URL == null)
        {
            return BadRequest("Options.URL is required.");
        }
        if (!PGLN.TryParse(request.PGLN, out var pgln, out var pglnErr))
        {
            return BadRequest(pglnErr);
        }

        var report = new DiagnosticsReport();
        var client = _httpClientFactory.CreateClient("default");
        var url = await EPCISTraceabilityResolver.GetEPCISQueryInterfaceURL(request.Options, pgln, client, report);
        var envelope = new DiagnosticsEnvelope<ResolvedUrlResult>
        {
            Data = new ResolvedUrlResult { EpcisQueryInterfaceUrl = url },
            Diagnostics = report
        };
        return Ok(envelope);
    }
}
