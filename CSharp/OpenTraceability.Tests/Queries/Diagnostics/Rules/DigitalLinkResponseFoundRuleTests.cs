using System;
using System.Linq;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
[Category("UnitTest")]
public class DigitalLinkResponseFoundRuleTests
{
    private DigitalLinkResponseFoundRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new DigitalLinkResponseFoundRule();
    }

    [Test]
    public void ExecuteAsync_WithNoParameters_ShouldThrowArgumentException()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync());
        Assert.That(ex?.Message, Does.Contain("required"));
    }

    [Test]
    public void ExecuteAsync_WithInvalidParameterType_ShouldThrowArgumentException()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync(new object()));
        Assert.That(ex?.Message, Does.Contain("invalid"));
    }

    [Test]
    public async Task ExecuteAsync_WithNullJson_ShouldReturnErrorResult()
    {
        var results = await _rule.ExecuteAsync((string?)null);

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("null or empty"));
    }

    [Test]
    public async Task ExecuteAsync_WithEmptyJson_ShouldReturnErrorResult()
    {
        var results = await _rule.ExecuteAsync(string.Empty);

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("null or empty"));
    }

    [Test]
    public async Task ExecuteAsync_WithValidJson_ShouldReturnNoResults()
    {
        const string json = "[ { \"link\": \"https://example.com/epcis\", \"linkType\": \"gs1:epcis\" } ]";

        var results = await _rule.ExecuteAsync(json);

        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_WithMissingLinkProperty_ShouldReturnErrorResult()
    {
        const string json = "[ { \"linkType\": \"gs1:epcis\" } ]";

        var results = await _rule.ExecuteAsync(json);

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("missing required 'link' property"));
    }

    [Test]
    public async Task ExecuteAsync_WithInvalidUrl_ShouldReturnWarningResult()
    {
        const string json = "[ { \"link\": \"not-a-valid-url\", \"linkType\": \"gs1:epcis\" } ]";

        var results = await _rule.ExecuteAsync(json);

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("not a valid URL"));
    }

    [Test]
    public async Task ExecuteAsync_WithNonArrayJson_ShouldReturnError()
    {
        const string json = "{ \"link\": \"https://example.com/epcis\" }";

        var results = await _rule.ExecuteAsync(json);

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("Failed to parse Digital Link response as array"));
    }

    [Test]
    public async Task ExecuteAsync_WithNullDigitalLinksArray_ShouldReturnError()
    {
        const string json = "null";

        var results = await _rule.ExecuteAsync(json);

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("not a valid array of Digital Link objects"));
    }

    [Test]
    public async Task ExecuteAsync_WithMultipleLinks_ShouldValidateAll()
    {
        const string json = "[ { \"link\": \"https://example.com/epcis\", \"linkType\": \"gs1:epcis\" }, { \"linkType\": \"gs1:masterdata\" } ]";

        var results = await _rule.ExecuteAsync(json);

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Message, Does.Contain("missing required 'link' property"));
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_DL_RESPONSE_FOUND"));
    }
}
