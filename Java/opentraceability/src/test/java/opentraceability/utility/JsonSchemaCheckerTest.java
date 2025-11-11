package opentraceability.utility;

import opentraceability.utility.JsonSchemaChecker;
import opentraceability.utility.Pair;
import opentraceability.utility.EmbeddedResourceLoader;
import opentraceability.Setup;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.ValueSource;
import static org.junit.jupiter.api.Assertions.*;
import java.util.List;

public class JsonSchemaCheckerTest {

    @Test
    public void gdstJsonSchemaValidation() throws Exception {
        // read GDST test data
        EmbeddedResourceLoader loader = new EmbeddedResourceLoader();
        String jsonData = loader.readString(Setup.class, "/tests/gdst_data_withmasterdata.jsonld");

        // validate against GDST schema
        Pair<Boolean, List<String>> result = JsonSchemaChecker.isValid(jsonData, "GDST");

        // assert that there are no validation errors
        if (!result.getSecond().isEmpty()) {
            String errorMessage = String.join("\n", result.getSecond());
            fail("GDST JSON schema validation failed with " + result.getSecond().size() + " errors:\n" + errorMessage);
        }

        assertTrue(result.getFirst(), "GDST JSON should be valid against GDST schema");
    }

    @Test
    public void gdstDataAgainstBaseEpcisSchemaValidation() throws Exception {
        // read GDST test data
        EmbeddedResourceLoader loader = new EmbeddedResourceLoader();
        String jsonData = loader.readString(Setup.class, "/tests/gdst_data_withmasterdata.jsonld");

        // validate against base EPCIS schema
        Pair<Boolean, List<String>> result = JsonSchemaChecker.isValid(jsonData, "EPCIS_BASE");

        // output results for comparison
        if (!result.getSecond().isEmpty()) {
            String errorMessage = String.join("\n", result.getSecond());
            System.out.println("Base EPCIS validation failed with " + result.getSecond().size() + " errors:\n" + errorMessage);
            fail("GDST data should be valid against base EPCIS schema. Errors:\n" + errorMessage);
        }

        assertTrue(result.getFirst(), "GDST JSON should be valid against base EPCIS schema");
    }

    @ParameterizedTest
    @ValueSource(strings = {
        "gdst_hatching_event_invalid.jsonld",
        "gdst_fishing_event_invalid.jsonld", 
        "gdst_farm_harvest_event_invalid.jsonld",
        "gdst_feedmill_object_event_invalid.jsonld",
        "gdst_processing_event_invalid.jsonld"
    })
    public void gdstInvalidEventsShouldFailValidation(String testFile) throws Exception {
        // read invalid GDST test data
        EmbeddedResourceLoader loader = new EmbeddedResourceLoader();
        String jsonData = loader.readString(Setup.class, "/tests/" + testFile);

        // validate against GDST schema - should fail
        Pair<Boolean, List<String>> result = JsonSchemaChecker.isValid(jsonData, "GDST");

        // assert that validation fails with expected errors
        assertFalse(result.getFirst(), "Invalid GDST event should fail validation: " + testFile);
        assertFalse(result.getSecond().isEmpty(), "Should have validation errors for: " + testFile);
        
        System.out.println("Expected validation failure for " + testFile);
        System.out.println("Actual errors (" + result.getSecond().size() + "):");
        for (int i = 0; i < Math.min(5, result.getSecond().size()); i++) {
            System.out.println("  - " + result.getSecond().get(i));
        }
    }

    @ParameterizedTest
    @ValueSource(strings = {
        "gdst_hatching_missing_broodstock.jsonld",
        "gdst_event_missing_product_owner.jsonld",
        "gdst_event_missing_information_provider.jsonld",
        "gdst_event_invalid_human_welfare_policy.jsonld",
        "gdst_certification_invalid_type.jsonld",
        "gdst_vessel_invalid_imo.jsonld",
        "gdst_vessel_invalid_country_code.jsonld",
        "gdst_farm_harvest_missing_aquaculture_method.jsonld",
        "gdst_feedmill_missing_protein_source.jsonld",
        "gdst_processing_missing_lot_number.jsonld"
    })
    public void gdstRequiredPropertiesValidation(String testFile) throws Exception {
        // read invalid GDST test data
        EmbeddedResourceLoader loader = new EmbeddedResourceLoader();
        String jsonData = loader.readString(Setup.class, "/tests/" + testFile);

        // validate against GDST schema - should fail
        Pair<Boolean, List<String>> result = JsonSchemaChecker.isValid(jsonData, "GDST");

        // assert that validation fails with expected errors
        assertFalse(result.getFirst(), "Invalid GDST event should fail validation: " + testFile);
        assertFalse(result.getSecond().isEmpty(), "Should have validation errors for: " + testFile);
        
        System.out.println("Expected validation failure for " + testFile);
        System.out.println("Actual errors (" + result.getSecond().size() + "):");
        for (int i = 0; i < Math.min(3, result.getSecond().size()); i++) {
            System.out.println("  - " + result.getSecond().get(i));
        }
    }
}
