package models.events

import interfaces.IEventKDE
import java.util.*
import models.identifiers.*
import models.events.kdes.CertificationList
import models.events.*
import java.time.Duration
import java.lang.reflect.Type
import java.net.URI
import java.time.OffsetDateTime

class TransformationEvent<T> {
    var Inputs: List<EventProduct> = ArrayList<EventProduct>()
    var Outputs: List<EventProduct> = ArrayList<EventProduct>()
    var Action: EventAction? = null
    var TransformationID: String = String()
    var BusinessStep: URI? = null
    var Disposition: URI? = null
    var ReadPoint: EventReadPoint = EventReadPoint()
    var Location: EventLocation = EventLocation()
    var BizTransactionList: List<EventBusinessTransaction> = ArrayList<EventBusinessTransaction>()
    var SourceList: List<EventSource> = ArrayList<EventSource>()
    var DestinationList: List<EventDestination> = ArrayList<EventDestination>()
    var SensorElementList: List<SensorElement> = ArrayList<SensorElement>()
    var PersistentDisposition: PersistentDisposition = PersistentDisposition()
    var ILMD: T = T()
    var EventType: EventType = EventType()
    var Products: List<EventProduct> = ArrayList<EventProduct>()
    var EventTime: OffsetDateTime? = null
    var RecordTime: OffsetDateTime? = null
    var EventTimeZoneOffset: Duration? = null
    var EventID: URI? = null
    var ErrorDeclaration: ErrorDeclaration = ErrorDeclaration()
    var CertificationInfo: String = String()
    var CertificationList: CertificationList = CertificationList()
    var InformationProvider: PGLN = PGLN()
    var KDEs: List<IEventKDE> = ArrayList<IEventKDE>()

    companion object {
    }
}
