using System;
using System.Linq;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
[Category("UnitTest")]
public class DigitalLinkJsonSchemaRuleTests
{
    private DigitalLinkJsonSchemaRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new DigitalLinkJsonSchemaRule();
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
    public async Task ExecuteAsync_WithInvalidJson_ShouldReturnFormatError()
    {
        var results = await _rule.ExecuteAsync("{ invalid json");

        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("Invalid JSON format"));
    }

    [Test]
    public async Task ExecuteAsync_WithSchemaWarnings_ShouldIncludeWarningResults()
    {
        const string json = "[ { \"linkType\": \"gs1:epcis\" } ]";

        var results = await _rule.ExecuteAsync(json);

        Assert.That(results.Any(r => r.Level == LogLevel.Warning && r.Message.Contains("JSON schema validation warning")), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithValidJson_ShouldReturnNoResults()
    {
        const string json = "[ { \"link\": \"https://example.com/epcis\", \"linkType\": \"gs1:epcis\" } ]";

        var results = await _rule.ExecuteAsync(json);

        Assert.That(results.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task ExecuteAsync_WithValidDigitalLinkStructure_ShouldOnlyReturnSchemaWarnings()
    {
        const string json = "[ { \"link\": \"https://example.com/epcis\", \"linkType\": \"gs1:epcis\" } ]";

        var results = await _rule.ExecuteAsync(json);

        // Should only have JSON schema warnings, no DigitalLink structure errors
        Assert.That(results.All(r => r.Level == LogLevel.Warning && r.Message.Contains("JSON schema validation warning")), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithNonArrayJson_ShouldReturnSchemaWarnings()
    {
        const string json = "{ \"link\": \"https://example.com/epcis\" }";

        var results = await _rule.ExecuteAsync(json);

        // Should only return JSON schema warnings, not DigitalLink structure errors
        Assert.That(results.Any(r => r.Level == LogLevel.Warning && r.Message.Contains("JSON schema validation warning")), Is.True);
        Assert.That(results.Any(r => r.Message.Contains("Failed to parse Digital Link response as array")), Is.False);
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_DL_JSON_SCHEMA"));
    }
}
