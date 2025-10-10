using OpenTraceability.Mappers;
using OpenTraceability.Models.Events;
using OpenTraceability.Queries;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
public class EPCISResponseSchemaRuleTests
{
    private EPCISResponseSchemaRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new EPCISResponseSchemaRule();
    }

    [Test]
    public async Task ExecuteAsync_WithValidEPCISJson_ShouldPassValidation()
    {
        // Arrange
        var validEpcisJson = @"{
            ""@context"": [""https://ref.gs1.org/standards/epcis/epcis-context.jsonld""],
            ""type"": ""EPCISQueryDocument"",
            ""schemaVersion"": ""2.0"",
            ""creationDate"": ""2024-01-01T00:00:00.000Z"",
            ""epcisBody"": {
                ""queryResults"": {
                    ""resultsBody"": {
                        ""eventList"": []
                    }
                }
            }
        }";

        // Act
        var results = await _rule.ExecuteAsync(validEpcisJson, EPCISDataFormat.JSON, EPCISVersion.V2);

        // Assert
        Assert.That(results, Is.Not.Null);
        // Schema validation may return warnings, but should not have critical errors that prevent processing
        Assert.That(results.Any(r => r.Level == LogLevel.Error && r.Type == DiagnosticsValidationType.GeneralError), Is.False);
    }

    [Test]
    public async Task ExecuteAsync_WithValidEPCISXml_ShouldPassValidation()
    {
        // Arrange
        var validEpcisXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <epcis:EPCISQueryDocument xmlns:epcis=""urn:epcglobal:epcis:xsd:2"">
                <EPCISBody>
                    <QueryResults>
                        <resultsBody>
                            <EventList>
                            </EventList>
                        </resultsBody>
                    </QueryResults>
                </EPCISBody>
            </epcis:EPCISQueryDocument>";

        // Act
        var results = await _rule.ExecuteAsync(validEpcisXml, EPCISDataFormat.XML, EPCISVersion.V2);

        // Assert
        Assert.That(results, Is.Not.Null);
        // XML should parse successfully
        Assert.That(results.Any(r => r.Message.Contains("Invalid XML content")), Is.False);
    }

    [Test]
    public async Task ExecuteAsync_WithInvalidJson_ShouldReturnFormatError()
    {
        // Arrange
        var invalidJson = @"{ invalid json";

        // Act
        var results = await _rule.ExecuteAsync(invalidJson, EPCISDataFormat.JSON, EPCISVersion.V2);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results.Any(r => r.Type == DiagnosticsValidationType.SchemaError), Is.True);
        Assert.That(results.Any(r => r.Message.Contains("Invalid JSON content")), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithInvalidXml_ShouldReturnFormatError()
    {
        // Arrange
        var invalidXml = @"<invalid xml";

        // Act
        var results = await _rule.ExecuteAsync(invalidXml, EPCISDataFormat.XML, EPCISVersion.V2);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results.Any(r => r.Type == DiagnosticsValidationType.SchemaError), Is.True);
        Assert.That(results.Any(r => r.Message.Contains("Invalid XML content")), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithNullOrEmptyContent_ShouldReturnError()
    {
        // Act
        var resultsNull = await _rule.ExecuteAsync(null, EPCISDataFormat.JSON, EPCISVersion.V2);
        var resultsEmpty = await _rule.ExecuteAsync("", EPCISDataFormat.JSON, EPCISVersion.V2);

        // Assert
        Assert.That(resultsNull.Count, Is.EqualTo(1));
        Assert.That(resultsNull[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(resultsNull[0].Message, Does.Contain("null or empty"));

        Assert.That(resultsEmpty.Count, Is.EqualTo(1));
        Assert.That(resultsEmpty[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
    }

    [Test]
    public void ExecuteAsync_WithInsufficientParameters_ShouldThrowArgumentException()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync("content"));
        Assert.That(ex?.Message, Does.Contain("requires 3 parameters"));
    }

    [Test]
    public void ExecuteAsync_WithInvalidParameterTypes_ShouldThrowArgumentException()
    {
        var ex1 = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync(123, EPCISDataFormat.JSON, EPCISVersion.V2));
        Assert.That(ex1?.Message, Does.Contain("Parameter 0"));
        var ex2 = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync("{}", "notFormat", EPCISVersion.V2));
        Assert.That(ex2?.Message, Does.Contain("Parameter 1"));
        var ex3 = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync("{}", EPCISDataFormat.JSON, "notVersion"));
        Assert.That(ex3?.Message, Does.Contain("Parameter 2"));
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        // Assert
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_EPCIS_RESPONSE_SCHEMA"));
    }
}
