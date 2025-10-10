using System.Net.Http;
using System.Net.Http.Headers;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
[Category("UnitTest")]
public class DigitalLinkHttpRequestRuleTests
{
    private DigitalLinkHttpRequestRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new DigitalLinkHttpRequestRule();
    }

    [Test]
    public async Task ExecuteAsync_WithValidHeaders_ShouldPassValidation()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Host = "example.com";
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        var results = await _rule.ExecuteAsync(request.Headers);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No validation errors
    }

    [Test]
    public async Task ExecuteAsync_WithMissingAcceptHeader_ShouldReturnError()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Host = "example.com";
        // No Accept header added

        // Act
        var results = await _rule.ExecuteAsync(request.Headers);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.HttpError));
        Assert.That(results[0].RuleKey, Is.EqualTo("OT_DIAG_RULE_DL_HTTP_REQUEST"));
        Assert.That(results[0].Message, Does.Contain("Accept header is missing"));
    }

    [Test]
    public async Task ExecuteAsync_WithInvalidAcceptHeader_ShouldReturnError()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Host = "example.com";
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml")); // Wrong content type

        // Act
        var results = await _rule.ExecuteAsync(request.Headers);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.HttpError));
        Assert.That(results[0].Message, Does.Contain("application/json"));
    }

    [Test]
    public async Task ExecuteAsync_WithMissingHostHeader_ShouldReturnError()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // No Host header set

        // Act
        var results = await _rule.ExecuteAsync(request.Headers);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.HttpError));
        Assert.That(results[0].Message, Does.Contain("Host header is missing"));
    }

    [Test]
    public void ExecuteAsync_WithNullParameter_ShouldThrowException()
    {
        // Act & Assert
        HttpRequestHeaders nullHeaders = null!;
        var exception = Assert.ThrowsAsync<ArgumentNullException>(async () => await _rule.ExecuteAsync(nullHeaders));
        Assert.That(exception.Message, Does.Contain("null or invalid"));
    }

    [Test]
    public void ExecuteAsync_WithNoParameters_ShouldThrowException()
    {
        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync());
        Assert.That(exception.Message, Does.Contain("parameter is required"));
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        // Assert
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_DL_HTTP_REQUEST"));
    }
}
