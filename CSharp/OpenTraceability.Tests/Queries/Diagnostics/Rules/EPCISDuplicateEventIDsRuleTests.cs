using OpenTraceability.Models.Events;
using OpenTraceability.Models.Identifiers;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
[Category("UnitTest")]
public class EPCISDuplicateEventIDsRuleTests
{
    private EPCISDuplicateEventIDsRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new EPCISDuplicateEventIDsRule();
    }

    [Test]
    public async Task ExecuteAsync_WithUniqueEventIds_ShouldPassValidation()
    {
        // Arrange
        var document = new EPCISQueryDocument();
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = new Uri("urn:uuid:event1") });
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = new Uri("urn:uuid:event2") });
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = new Uri("urn:uuid:event3") });

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No validation errors
    }

    [Test]
    public async Task ExecuteAsync_WithDuplicateEventIds_ShouldReturnErrors()
    {
        // Arrange
        var document = new EPCISQueryDocument();
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = new Uri("urn:uuid:duplicate") });
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = new Uri("urn:uuid:unique") });
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = new Uri("urn:uuid:duplicate") }); // Duplicate

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results.Any(r => r.Message.Contains("Duplicate event ID")), Is.True);
        Assert.That(results.Any(r => r.Message.Contains("urn:uuid:duplicate")), Is.True);
        Assert.That(results.Any(r => r.Level == LogLevel.Error), Is.True);
        Assert.That(results.Any(r => r.Type == DiagnosticsValidationType.BusinessRuleError), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithMissingEventIds_ShouldReturnWarnings()
    {
        // Arrange
        var document = new EPCISQueryDocument();
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = new Uri("urn:uuid:valid") });
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = null }); // Missing EventID
        document.Events.Add(new ObjectEvent<EventILMD> { EventID = null }); // Missing EventID

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results.Any(r => r.Message.Contains("missing required eventID")), Is.True);
        Assert.That(results.Any(r => r.Message.Contains("2 event(s)")), Is.True);
        Assert.That(results.Any(r => r.Type == DiagnosticsValidationType.SchemaError), Is.True);
        Assert.That(results.Any(r => r.Level == LogLevel.Warning), Is.True); // now a warning
    }

    [Test]
    public async Task ExecuteAsync_WithEmptyDocument_ShouldPassValidation()
    {
        // Arrange
        var document = new EPCISQueryDocument(); // No events

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No validation errors for empty document
    }

    [Test]
    public void ExecuteAsync_WithNullDocument_ShouldThrow()
    {
        // Act / Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync((object?)null));
        Assert.That(ex!.Message, Does.Contain("EPCISBaseDocument"));
    }

    [Test]
    public void ExecuteAsync_WithNoParameters_ShouldThrow()
    {
        // Act / Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync());
        Assert.That(ex!.Message, Does.Contain("Expected exactly one parameter"));
    }

    [Test]
    public void ExecuteAsync_WithTooManyParameters_ShouldThrow()
    {
        // Arrange
        var document = new EPCISQueryDocument();

        // Act / Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync(document, document));
        Assert.That(ex!.Message, Does.Contain("Expected exactly one parameter"));
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        // Assert
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_EPCIS_DUPLICATE_EVENT_IDS"));
    }
}
