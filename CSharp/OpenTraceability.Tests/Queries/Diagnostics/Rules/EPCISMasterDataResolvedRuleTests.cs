using OpenTraceability.Models.Events;
using OpenTraceability.Models.Identifiers;
using OpenTraceability.Models.MasterData;
using OpenTraceability.Models.Common;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
public class EPCISMasterDataResolvedRuleTests
{
    private EPCISMasterDataResolvedRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new EPCISMasterDataResolvedRule();
    }

    [Test]
    public async Task ExecuteAsync_WithFullyResolvedMasterData_ShouldPassValidation()
    {
        // Arrange
        var gln = new GLN("1234567890128"); // Valid GLN with correct checksum
        var gtin = new GTIN("urn:gdst:example.org:product:class:feedmill.1u"); // Use GDST format GTIN
        var pgln = new PGLN("9876543210982"); // Valid PGLN with correct checksum

        var document = new EPCISQueryDocument();

        // Add event with location, product, and source
        var evt = new ObjectEvent<EventILMD>
        {
            EventID = new Uri("urn:uuid:event1"),
            Location = new EventLocation { GLN = gln }
        };
        var product = new EventProduct(new EPC("urn:gdst:example.org:product:serial:obj:feedmill.1u.12345"));
        product.Type = EventProductType.Reference;
        evt.AddProduct(product);
        evt.SourceList.Add(new EventSource { Type = OpenTraceability.Constants.EPCIS.URN.SDT_Owner, Value = pgln.ToString() });
        document.Events.Add(evt);

        // Add corresponding master data
        document.MasterData.Add(new Location { GLN = gln, Name = new List<LanguageString> { new LanguageString { Language = "en", Value = "Test Location" } } });
        document.MasterData.Add(new Tradeitem { GTIN = gtin, ShortDescription = new List<LanguageString> { new LanguageString { Language = "en", Value = "Test Product" } } });
        document.MasterData.Add(new TradingParty { PGLN = pgln, Name = new List<LanguageString> { new LanguageString { Language = "en", Value = "Test Trading Party" } } });

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        // Should have no warnings about unresolved master data
        Assert.That(results.Count, Is.EqualTo(0)); // All master data resolved
    }

    [Test]
    public async Task ExecuteAsync_WithMissingLocationMasterData_ShouldReturnInfo()
    {
        // Arrange
        var gln = new GLN("1234567890128"); // Valid GLN with correct checksum
        var document = new EPCISQueryDocument();

        var evt = new ObjectEvent<EventILMD>
        {
            EventID = new Uri("urn:uuid:event1"),
            Location = new EventLocation { GLN = gln }
        };
        document.Events.Add(evt);
        // No corresponding Location master data added

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.GreaterThan(0));
        Assert.That(results.Any(r => r.Message.Contains("Location master data not resolved")), Is.True);
        Assert.That(results.Any(r => r.Level == LogLevel.Info), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithMissingTradingPartyMasterData_ShouldReturnInfo()
    {
        // Arrange
        var pgln = new PGLN("9876543210982"); // Valid PGLN with correct checksum
        var document = new EPCISQueryDocument();

        var evt = new ObjectEvent<EventILMD>
        {
            EventID = new Uri("urn:uuid:event1")
        };
        evt.SourceList.Add(new EventSource { Type = OpenTraceability.Constants.EPCIS.URN.SDT_Owner, Value = pgln.ToString() });
        document.Events.Add(evt);
        // No corresponding TradingParty master data added

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Any(r => r.Message.Contains("TradingParty master data not resolved")), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithMissingProductMasterData_ShouldReturnInfo()
    {
        // Arrange
        var gtin = new GTIN("urn:gdst:example.org:product:class:feedmill.1u"); // Use GDST format GTIN
        var document = new EPCISQueryDocument();

        var evt = new ObjectEvent<EventILMD>
        {
            EventID = new Uri("urn:uuid:event1")
        };
        var product = new EventProduct(new EPC("urn:gdst:example.org:product:serial:obj:feedmill.1u.12345"));
        product.Type = EventProductType.Reference;
        evt.AddProduct(product);
        document.Events.Add(evt);
        // No corresponding Tradeitem master data added

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Any(r => r.Message.Contains("Tradeitem master data not resolved")), Is.True);
        Assert.That(results.Any(r => r.Level == LogLevel.Info), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithRequiredMasterDataFlag_ShouldReturnWarnings()
    {
        // Arrange
        var gln = new GLN("1234567890128"); // Valid GLN with correct checksum
        var document = new EPCISQueryDocument();

        var evt = new ObjectEvent<EventILMD>
        {
            EventID = new Uri("urn:uuid:event1"),
            Location = new EventLocation { GLN = gln }
        };
        document.Events.Add(evt);

        // Act
        var results = await _rule.ExecuteAsync(document, true); // requireMasterData = true

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Any(r => r.Level == LogLevel.Warning), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_WithMasterDataWithNullId_ShouldReturnError()
    {
        // Arrange
        var document = new EPCISQueryDocument();
        document.MasterData.Add(new Location { GLN = null }); // ID is null

        // Act
        var results = await _rule.ExecuteAsync(document);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Any(r => r.Message.Contains("has null ID")), Is.True);
        Assert.That(results.Any(r => r.Level == LogLevel.Error), Is.True);
        Assert.That(results.Any(r => r.Type == DiagnosticsValidationType.BusinessRuleError), Is.True);
    }

    [Test]
    public void ExecuteAsync_WithNullDocument_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await _rule.ExecuteAsync((object?)null));
        Assert.That(ex!.Message, Does.Contain("null or invalid"));
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
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_EPCIS_MASTER_DATA_RESOLVED"));
    }
}
