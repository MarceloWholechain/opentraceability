using System.Net;
using System.Net.Http;
using System.Text;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
public class DigitalLinkHttpResponseRuleTests
{
    private DigitalLinkHttpResponseRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new DigitalLinkHttpResponseRule();
    }

    [Test]
    public async Task ExecuteAsync_WithSuccessStatusAndJsonContentType_ShouldPassValidation()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"test\": \"value\"}", Encoding.UTF8, "application/json")
        };

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No warnings or errors
    }

    [Test]
    public async Task ExecuteAsync_WithNonSuccessStatusCode_ShouldReturnError()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        var errorResult = results.FirstOrDefault(r => r.Level == LogLevel.Error);
        Assert.That(errorResult, Is.Not.Null);
        Assert.That(errorResult.Type, Is.EqualTo(DiagnosticsValidationType.HttpError));
        Assert.That(errorResult.Message, Does.Contain("non-success status code"));
        Assert.That(errorResult.Message, Does.Contain("404"));
    }

    [Test]
    public async Task ExecuteAsync_WithMissingContentType_ShouldReturnWarning()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"test\": \"value\"}")
        };
        response.Content.Headers.ContentType = null;

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        var warningResult = results.FirstOrDefault(r => r.Level == LogLevel.Warning);
        Assert.That(warningResult, Is.Not.Null);
        Assert.That(warningResult.Type, Is.EqualTo(DiagnosticsValidationType.HttpError));
        Assert.That(warningResult.Message, Does.Contain("Content-Type header is missing"));
    }

    [Test]
    public async Task ExecuteAsync_WithIncorrectContentType_ShouldReturnWarning()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("<xml>test</xml>", Encoding.UTF8, "application/xml")
        };

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        var warningResult = results.FirstOrDefault(r => r.Level == LogLevel.Warning);
        Assert.That(warningResult, Is.Not.Null);
        Assert.That(warningResult.Type, Is.EqualTo(DiagnosticsValidationType.HttpError));
        Assert.That(warningResult.Message, Does.Contain("Unexpected Content-Type header"));
        Assert.That(warningResult.Message, Does.Contain("application/xml"));
    }

    [Test]
    public async Task ExecuteAsync_WithErrorStatusAndIncorrectContentType_ShouldReturnBothErrorAndWarning()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("Error", Encoding.UTF8, "text/plain")
        };

        // Act
        var results = await _rule.ExecuteAsync(response);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(2));

        var errorResult = results.FirstOrDefault(r => r.Level == LogLevel.Error);
        Assert.That(errorResult, Is.Not.Null);
        Assert.That(errorResult.Message, Does.Contain("500"));

        var warningResult = results.FirstOrDefault(r => r.Level == LogLevel.Warning);
        Assert.That(warningResult, Is.Not.Null);
        Assert.That(warningResult.Message, Does.Contain("text/plain"));
    }

    [Test]
    public void ExecuteAsync_WithNullParameter_ShouldThrowException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync(null!));
        Assert.That(ex.Message, Does.Contain("required"));
    }

    [Test]
    public void ExecuteAsync_WithNoParameters_ShouldThrowException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync());
        Assert.That(ex.Message, Does.Contain("parameter is required"));
    }

    [Test]
    public void ExecuteAsync_WithWrongParameterType_ShouldThrowException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync("invalid parameter"));
        Assert.That(ex.Message, Does.Contain("null or invalid"));
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        // Assert
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_DL_HTTP_RESPONSE"));
    }
}
