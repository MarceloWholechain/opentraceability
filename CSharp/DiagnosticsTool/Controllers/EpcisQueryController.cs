using DiagnosticsTool.Models.Requests;
using DiagnosticsTool.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using OpenTraceability.Models.Identifiers;
using OpenTraceability.Queries;
using OpenTraceability.Queries.Diagnostics;

namespace DiagnosticsTool.Controllers;

[ApiController]
[Route("api/epcis")] 
public class EpcisQueryController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EpcisQueryController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("query/events")] 
    public async Task<ActionResult<DiagnosticsEnvelope<EPCISQueryResults>>> QueryEvents([FromBody] QueryEventsRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is required.");
        }
        if (request.Options?.URL == null)
        {
            return BadRequest("Options.URL is required.");
        }
        request.Parameters ??= new EPCISQueryParameters();

        var report = new DiagnosticsReport();
        var client = _httpClientFactory.CreateClient("default");
        var results = await EPCISTraceabilityResolver.QueryEvents(request.Options, request.Parameters, client, report);
        var envelope = new DiagnosticsEnvelope<EPCISQueryResults>
        {
            Data = results,
            Diagnostics = report
        };
        return Ok(envelope);
    }

    [HttpPost("query/traceback")] 
    public async Task<ActionResult<DiagnosticsEnvelope<EPCISQueryResults>>> Traceback([FromBody] TracebackRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.EPC))
        {
            return BadRequest("EPC is required.");
        }
        if (request.Options?.URL == null)
        {
            return BadRequest("Options.URL is required.");
        }
        if (!EPC.TryParse(request.EPC, out var epc, out var epcErr))
        {
            return BadRequest(epcErr);
        }

        var report = new DiagnosticsReport();
        var client = _httpClientFactory.CreateClient("default");
        var results = await EPCISTraceabilityResolver.Traceback(request.Options, epc, client, request.AdditionalParameters, report);
        var envelope = new DiagnosticsEnvelope<EPCISQueryResults>
        {
            Data = results,
            Diagnostics = report
        };
        return Ok(envelope);
    }
}
