using OpenTraceability.Mappers.EPCIS.XML;
using OpenTraceability.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenTraceability.Tests.Events
{
    [TestFixture]
    public class SchemaCheckTests
    {
        [Test]
        [TestCase("querydoc_fail_schemacheck.xml", false)]
        public void EPCISQueryDocument_XML_1_2(string file, bool pass)
        {
            // read object events from test data specified in the file argument
            string xmlObjectEvents = OpenTraceabilityTests.ReadTestData(file);

            XDocument xDoc = XDocument.Parse(xmlObjectEvents);
            try
            {
                EPCISDocumentBaseXMLMapper.ValidateEPCISQueryDocumentSchema(xDoc, Models.Events.EPCISVersion.V1);
                Assert.That(pass, Is.True);
            }
            catch (Exception)
            {
                Assert.That(pass, Is.False);
            }
        }

        [Test]
        public async Task GDST_JSON_Schema_Validation()
        {
            // read GDST test data
            string jsonData = OpenTraceabilityTests.ReadTestData("gdst_data_withmasterdata.jsonld");

            // validate against GDST schema
            List<string> errors = await JsonSchemaChecker.IsValidAsync(jsonData, "GDST");

            // assert that there are no validation errors
            if (errors.Count > 0)
            {
                string errorMessage = string.Join("\n", errors);
                Assert.Fail($"GDST JSON schema validation failed with {errors.Count} errors:\n{errorMessage}");
            }

            Assert.That(errors.Count, Is.EqualTo(0), "GDST JSON should be valid against GDST schema");
        }

        [Test]
        public async Task GDST_Data_Against_Base_EPCIS_Schema_Validation()
        {
            // read GDST test data
            string jsonData = OpenTraceabilityTests.ReadTestData("gdst_data_withmasterdata.jsonld");

            // validate against base EPCIS schema
            List<string> errors = await JsonSchemaChecker.IsValidAsync(jsonData, "EPCIS_BASE");

            // output results for comparison
            if (errors.Count > 0)
            {
                string errorMessage = string.Join("\n", errors);
                Console.WriteLine($"Base EPCIS validation failed with {errors.Count} errors:\n{errorMessage}");
                Assert.Fail($"GDST data should be valid against base EPCIS schema. Errors:\n{errorMessage}");
            }

            Assert.That(errors.Count, Is.EqualTo(0), "GDST JSON should be valid against base EPCIS schema");
        }

        [Test]
        [TestCase("gdst_hatching_event_invalid.jsonld", "Invalid broodstockSource enum value should fail")]
        [TestCase("gdst_fishing_event_invalid.jsonld", "Missing vessel information should fail")]
        [TestCase("gdst_farm_harvest_event_invalid.jsonld", "Invalid production method should fail")]
        [TestCase("gdst_feedmill_object_event_invalid.jsonld", "Missing gdst:proteinSource should fail")]
        [TestCase("gdst_processing_event_invalid.jsonld", "Missing required lotNumber should fail")]
        public async Task GDST_Invalid_Events_Should_Fail_Validation(string testFile, string expectedFailureReason)
        {
            // read invalid GDST test data
            string jsonData = OpenTraceabilityTests.ReadTestData(testFile);

            // validate against GDST schema - should fail
            List<string> errors = await JsonSchemaChecker.IsValidAsync(jsonData, "GDST");

            // assert that validation fails with expected errors
            Assert.That(errors.Count, Is.GreaterThan(0), $"Invalid GDST event should fail validation: {expectedFailureReason}");
            
            Console.WriteLine($"Expected validation failure for {testFile}: {expectedFailureReason}");
            Console.WriteLine($"Actual errors ({errors.Count}):");
            foreach (string error in errors.Take(5)) // Show first 5 errors
            {
                Console.WriteLine($"  - {error}");
            }
        }

        [Test]
        [TestCase("gdst_hatching_missing_broodstock.jsonld", "Hatching event missing gdst:broodstockSource should fail")]
        [TestCase("gdst_event_missing_product_owner.jsonld", "GDST event missing gdst:productOwner should fail")]
        [TestCase("gdst_event_missing_information_provider.jsonld", "GDST event missing cbvmda:informationProvider should fail")]
        [TestCase("gdst_event_invalid_human_welfare_policy.jsonld", "GDST event with invalid humanWelfarePolicy should fail")]
        [TestCase("gdst_certification_invalid_type.jsonld", "GDST event with invalid certification type pattern should fail")]
        [TestCase("gdst_vessel_invalid_imo.jsonld", "Fishing event with invalid IMO number pattern should fail")]
        [TestCase("gdst_vessel_invalid_country_code.jsonld", "Fishing event with invalid 3-letter country code should fail")]
        [TestCase("gdst_farm_harvest_missing_aquaculture_method.jsonld", "Farm harvest event missing gdst:aquacultureMethod should fail")]
        [TestCase("gdst_feedmill_missing_protein_source.jsonld", "Feedmill event missing gdst:proteinSource should fail")]
        [TestCase("gdst_processing_missing_lot_number.jsonld", "Processing event missing cbvmda:lotNumber should fail")]
        public async Task GDST_Required_Properties_Validation(string testFile, string expectedFailureReason)
        {
            // read invalid GDST test data
            string jsonData = OpenTraceabilityTests.ReadTestData(testFile);

            // validate against GDST schema - should fail
            List<string> errors = await JsonSchemaChecker.IsValidAsync(jsonData, "GDST");

            // assert that validation fails with expected errors
            Assert.That(errors.Count, Is.GreaterThan(0), $"Invalid GDST event should fail validation: {expectedFailureReason}");
            
            Console.WriteLine($"Expected validation failure for {testFile}: {expectedFailureReason}");
            Console.WriteLine($"Actual errors ({errors.Count}):");
            foreach (string error in errors.Take(3)) // Show first 3 errors
            {
                Console.WriteLine($"  - {error}");
            }
        }

    }
}