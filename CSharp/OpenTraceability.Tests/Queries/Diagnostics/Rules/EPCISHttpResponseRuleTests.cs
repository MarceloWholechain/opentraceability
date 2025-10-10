using System.Net;
using System.Net.Http;
using System.Text;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
[Category("UnitTest")]
public class EPCISHttpResponseRuleTests
{
    private EPCISHttpResponseRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new EPCISHttpResponseRule();
    }

    [Test]
    public async Task ExecuteAsync_WithSuccessStatusCode_ShouldPassValidation()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent(@"{""epcisBody"":{""eventList"":[]}}",
            Encoding.UTF8, "application/json");

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No validation errors for success response
    }

    [Test]
    public async Task ExecuteAsync_WithErrorStatusCode_ShouldReturnError()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.HttpError));
        Assert.That(results[0].RuleKey, Is.EqualTo("OT_DIAG_RULE_EPCIS_HTTP_RESPONSE"));
        Assert.That(results[0].Message, Does.Contain("404"));
        Assert.That(results[0].Message, Does.Contain("NotFound"));
    }

    [Test]
    public async Task ExecuteAsync_WithServerError_ShouldReturnError()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.HttpError));
        Assert.That(results[0].Message, Does.Contain("500"));
    }

    [Test]
    public async Task ExecuteAsync_WithUnexpectedContentType_ShouldReturnWarning()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent("content", Encoding.UTF8, "text/plain"); // Unexpected content type

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        // Should have a warning about unexpected content type
        Assert.That(results.Any(r => r.Level == LogLevel.Warning &&
            r.Message.Contains("Unexpected Content-Type")), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithMissingContentType_ShouldReturnWarning()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        // No content set, so no Content-Type header

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Any(r => r.Message.Contains("Content-Type header is missing")), Is.True);
    }

    [Test]
    public void ExecuteAsync_WithNullParameter_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync((object?)null));
        Assert.That(ex!.Message, Does.Contain("null or invalid").Or.Contain("required"));
    }

    [Test]
    public void ExecuteAsync_WithNoParameters_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync());
        Assert.That(ex!.Message, Does.Contain("required"));
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        // Assert
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_EPCIS_HTTP_RESPONSE"));
    }
}
