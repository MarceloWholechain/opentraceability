using OpenTraceability.Models.Identifiers;
using OpenTraceability.Models.MasterData;
using OpenTraceability.Models.Common;
using OpenTraceability.Queries.Diagnostics;
using OpenTraceability.Queries.Diagnostics.Rules;

namespace OpenTraceability.Tests.Queries.Diagnostics.Rules;

[TestFixture]
[Category("UnitTest")]
public class MasterDataValidResponseRuleTests
{
    private MasterDataValidResponseRule _rule;

    [SetUp]
    public void SetUp()
    {
        _rule = new MasterDataValidResponseRule();
    }

    [Test]
    public async Task ExecuteAsync_WithMatchingGTINAndTradeitem_ShouldPassValidation()
    {
        // Arrange
        var gtin = new GTIN("01234567890128"); // Valid GTIN with correct checksum
        var tradeitem = new Tradeitem
        {
            GTIN = gtin,
            ShortDescription = new List<LanguageString> { new LanguageString { Language = "en", Value = "Test Product" } }
        };

        // Act
        var results = await _rule.ExecuteAsync(gtin, tradeitem);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No validation errors
    }

    [Test]
    public async Task ExecuteAsync_WithMismatchedGTINAndTradeitem_ShouldReturnError()
    {
        // Arrange
        var queryGtin = new GTIN("01234567890128"); // Valid GTIN with correct checksum
        var responseTradeitem = new Tradeitem
        {
            GTIN = new GTIN("98765432109879"), // Valid GTIN with correct checksum
            ShortDescription = new List<LanguageString> { new LanguageString { Language = "en", Value = "Different Product" } }
        };

        // Act
        var results = await _rule.ExecuteAsync(queryGtin, responseTradeitem);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.BusinessRuleError));
        Assert.That(results[0].Message, Does.Contain("does not match queried GTIN"));
    }

    [Test]
    public async Task ExecuteAsync_WithGTINButNonTradeitemResponse_ShouldReturnError()
    {
        // Arrange
        var gtin = new GTIN("01234567890128"); // Valid GTIN with correct checksum
        var wrongType = new Location(); // Wrong type

        // Act
        var results = await _rule.ExecuteAsync(gtin, wrongType);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.BusinessRuleError));
        Assert.That(results[0].Message, Does.Contain("Expected Tradeitem"));
    }

    [Test]
    public async Task ExecuteAsync_WithMatchingGLNAndLocation_ShouldPassValidation()
    {
        // Arrange
        var gln = new GLN("1234567890128"); // Valid GLN with correct checksum
        var location = new Location
        {
            GLN = gln,
            Name = new List<LanguageString> { new LanguageString { Language = "en", Value = "Test Location" } }
        };

        // Act
        var results = await _rule.ExecuteAsync(gln, location);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No validation errors
    }

    [Test]
    public async Task ExecuteAsync_WithMismatchedGLNAndLocation_ShouldReturnError()
    {
        // Arrange
        var queryGln = new GLN("1234567890128"); // Valid GLN with correct checksum
        var responseLocation = new Location
        {
            GLN = new GLN("9876543210982"), // Valid GLN with correct checksum
            Name = new List<LanguageString> { new LanguageString { Language = "en", Value = "Different Location" } }
        };

        // Act
        var results = await _rule.ExecuteAsync(queryGln, responseLocation);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.BusinessRuleError));
        Assert.That(results[0].Message, Does.Contain("does not match queried GLN"));
    }

    [Test]
    public async Task ExecuteAsync_WithMatchingPGLNAndTradingParty_ShouldPassValidation()
    {
        // Arrange
        var pgln = new PGLN("1234567890128"); // Valid PGLN with correct checksum
        var tradingParty = new TradingParty
        {
            PGLN = pgln,
            Name = new List<LanguageString> { new LanguageString { Language = "en", Value = "Test Trading Party" } }
        };

        // Act
        var results = await _rule.ExecuteAsync(pgln, tradingParty);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(0)); // No validation errors
    }

    [Test]
    public async Task ExecuteAsync_WithMismatchedPGLNAndTradingParty_ShouldReturnError()
    {
        // Arrange
        var queryPgln = new PGLN("1234567890128"); // Valid PGLN with correct checksum
        var responseTradingParty = new TradingParty
        {
            PGLN = new PGLN("9876543210982"), // Valid PGLN with correct checksum
            Name = new List<LanguageString> { new LanguageString { Language = "en", Value = "Different Trading Party" } }
        };

        // Act
        var results = await _rule.ExecuteAsync(queryPgln, responseTradingParty);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.BusinessRuleError));
        Assert.That(results[0].Message, Does.Contain("does not match queried PGLN"));
    }

    [Test]
    public async Task ExecuteAsync_WithNullMasterDataItem_ShouldReturnWarning()
    {
        // Arrange
        var gtin = new GTIN("01234567890128"); // Valid GTIN with correct checksum

        // Act
        var results = await _rule.ExecuteAsync(gtin, null);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.BusinessRuleError));
        Assert.That(results[0].Message, Does.Contain("returned is null"));
    }

    [Test]
    public async Task ExecuteAsync_WithUnsupportedIdentifierType_ShouldReturnWarning()
    {
        // Arrange
        var unsupportedIdentifier = "string-identifier";
        var tradeitem = new Tradeitem();

        // Act
        var results = await _rule.ExecuteAsync(unsupportedIdentifier, tradeitem);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Warning));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.GeneralError));
        Assert.That(results[0].Message, Does.Contain("Unsupported identifier type"));
    }

    [Test]
    public void ExecuteAsync_WithInsufficientParameters_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
            await _rule.ExecuteAsync(new GTIN("01234567890128"))); // Valid GTIN with correct checksum

        Assert.That(ex.Message, Does.Contain("Insufficient parameters"));
    }

    [Test]
    public void ExecuteAsync_WithNullIdentifier_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () => 
            await _rule.ExecuteAsync(null, new Tradeitem()));

        Assert.That(ex.Message, Does.Contain("Identifier parameter is null"));
    }

    [Test]
    public async Task ExecuteAsync_WithTradeitemWithoutGTIN_ShouldReturnError()
    {
        // Arrange
        var gtin = new GTIN("01234567890128"); // Valid GTIN with correct checksum
        var tradeitemWithoutGtin = new Tradeitem(); // No GTIN set

        // Act
        var results = await _rule.ExecuteAsync(gtin, tradeitemWithoutGtin);

        // Assert
        Assert.That(results, Is.Not.Null);
        Assert.That(results.Count, Is.EqualTo(1));
        Assert.That(results[0].Level, Is.EqualTo(LogLevel.Error));
        Assert.That(results[0].Type, Is.EqualTo(DiagnosticsValidationType.BusinessRuleError));
        Assert.That(results[0].Message, Does.Contain("does not match queried GTIN"));
    }

    [Test]
    public void Key_ShouldHaveCorrectValue()
    {
        // Assert
        Assert.That(_rule.Key, Is.EqualTo("OT_DIAG_RULE_MD_VALID_RESPONSE"));
    }
}
