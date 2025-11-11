using DiagnosticsTool.Models.Tests;
using DiagnosticsTool.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiagnosticsTool.Controllers;

[ApiController]
[Route("api/v1/test/{testId}")]
public class TestController : ControllerBase
{
    private readonly ITestService _testService;

    public TestController(ITestService testService)
    {
        _testService = testService;
    }

    [HttpGet("{*path}")]
    public IActionResult ProxyRequest(string testId, string? path)
    {
        if (string.IsNullOrWhiteSpace(testId))
        {
            return BadRequest("Test ID is required.");
        }

        var test = _testService.GetTest(testId);
        if (test == null)
        {
            return NotFound();
        }

        var relativePath = path ?? string.Empty;
        var request = test.FindRequest(relativePath);
        if (request == null)
        {
            return NotFound();
        }

        AttachResponseHeaders(request.Response);
        return new FileContentResult(request.Response.BodyBytes, request.Response.ContentType ?? "application/json");
    }

    private void AttachResponseHeaders(DiagnosticsToolTestResponse response)
    {
        Response.StatusCode = response.StatusCode;
        if (response.Headers != null)
        {
            foreach (var header in response.Headers)
            {
                Response.Headers[header.Key] = header.Value;
            }
        }

        if (!string.IsNullOrEmpty(response.ContentType))
        {
            Response.ContentType = response.ContentType;
        }
    }
}

