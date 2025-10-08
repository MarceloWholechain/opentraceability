using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
public class MasterDataJsonSchemaRuleTests
{
    private MasterDataJsonSchemaRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new MasterDataJsonSchemaRule();
    }

    [Test]
    public async Task ExecuteAsync_WithValidJsonLD_ShouldPassValidation()
    {
        // Arrange
        var validJsonLD = @"{
            ""@context"": ""https://gs1.org/voc/"",
            ""@type"": ""Tradeitem"",
            ""gtin"": ""01234567890123"",
            ""productName"": ""Test Product""
        }";

        // Act
        var results = await _rule.ExecuteAsync(validJsonLD);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No validation errors
    }

    [Test]
    public async Task ExecuteAsync_WithValidJsonWithoutContext_ShouldReturnInfo()
    {
        // Arrange
        var jsonWithoutContext = @"{
            ""gtin"": ""01234567890123"",
            ""productName"": ""Test Product""
        }";

        // Act
        var results = await _rule.ExecuteAsync(jsonWithoutContext);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Info));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("missing JSON-LD context"));
    }

    [Test]
    public async Task ExecuteAsync_WithInvalidJson_ShouldReturnError()
    {
        // Arrange
        var invalidJson = @"{ invalid json";

        // Act
        var results = await _rule.ExecuteAsync(invalidJson);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.SchemaError));
        Assert.That(results[0].Message, Does.Contain("Invalid JSON format"));
    }

    [Test]
    public void ExecuteAsync_WithNullContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync((string?)null));
        Assert.That(ex.Message, Does.Contain("null or empty"));
    }

    [Test]
    public void ExecuteAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync(""));
        Assert.That(ex.Message, Does.Contain("null or empty"));
    }

    [Test]
    public void ExecuteAsync_WithWhitespaceContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync("   "));
        Assert.That(ex.Message, Does.Contain("null or empty"));
    }

    [Test]
    public async Task ExecuteAsync_WithArrayJson_ShouldHandleGracefully()
    {
        // Arrange
        var arrayJson = @"[{""@context"": ""https://gs1.org/voc/"", ""@type"": ""Tradeitem""}]";

        // Act
        var results = await _rule.ExecuteAsync(arrayJson);

        // Assert
        Assert.That(results, Is.Not.Null);
        // Should handle parsing gracefully, even if it's an array instead of object
    }

    [Test]
    public void ExecuteAsync_WithNoParameters_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync());
        Assert.That(ex.Message, Does.Contain("parameter is required"));
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        // Assert
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_MD_JSON_SCHEMA"));
    }
}
