package opentraceability.mappers;

import gs1.mappers.epcis.*;
import opentraceability.interfaces.*;
import opentraceability.mappers.epcis.json.*;
import opentraceability.mappers.masterdata.*;
import opentraceability.models.events.*;
import opentraceability.*;

public class MasterDataMappers
{
	public IMasterDataMapper GS1WebVocab = new GS1VocabJsonMapper();
}
