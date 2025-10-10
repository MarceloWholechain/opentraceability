using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OpenTraceability.Mappers;
using OpenTraceability.Models.Events;
using OpenTraceability.Models.Identifiers;
using OpenTraceability.Queries;
using OpenTraceability.Queries.Diagnostics;

namespace OpenTraceability.Tests.Queries.Diagnostics
{
    [TestFixture]
    [Category("Integration")]
    public class DiagnosticsIntegrationTest
    {
        private static string ReadTestData(string filename) => OpenTraceabilityTests.ReadTestData(filename);

        private HttpClient BuildHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) => responder(req));
            return new HttpClient(handler.Object)
            {
                BaseAddress = new Uri("https://example.com/")
            };
        }

        private EPCISQueryInterfaceOptions CreateQueryOptions() => new EPCISQueryInterfaceOptions
        {
            URL = new Uri("https://example.com/epcis"),
            Version = EPCISVersion.V2,
            Format = EPCISDataFormat.JSON,
            EnableStackTrace = true
        };

        private DigitalLinkQueryOptions CreateDigitalLinkOptions() => new DigitalLinkQueryOptions
        {
            URL = new Uri("https://example.com/digitallink/")
        };

        // Use a valid EPC string (SSCC) to avoid GTIN parsing issues
        private EPC GetSampleEPC() => new EPC("urn:epc:id:sscc:08600031303.0004");

        [Test]
        public async Task DigitalLink_GetEPCISQueryInterfaceURL_HTTP500()
        {
            var report = new DiagnosticsReport();
            var epc = GetSampleEPC();
            var options = CreateDigitalLinkOptions();

            HttpClient client = BuildHttpClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });

            var result = await EPCISTraceabilityResolver.GetEPCISQueryInterfaceURL(options, epc, client, report);

            Assert.That(result, Is.Null);
            Assert.That(report.Requests.Count, Is.EqualTo(1));
            Assert.That(report.Requests[0].Validations.Any(v => v.Type == DiagnosticsValidationType.HttpError && v.Message.Contains("500")), Is.True);
        }

        [Test]
        public async Task QueryEvents_HTTP500()
        {
            var report = new DiagnosticsReport();
            var options = CreateQueryOptions();
            var epc = GetSampleEPC();
            var parameters = new EPCISQueryParameters(epc);

            HttpClient client = BuildHttpClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("{\"error\":\"server\"}", Encoding.UTF8, "application/json")
            });

            var results = await EPCISTraceabilityResolver.QueryEvents(options, parameters, client, report);

            Assert.Multiple(() =>
            {
                Assert.That(results.Errors.Count, Is.EqualTo(1));
                Assert.That(results.Errors[0].Details, Does.Contain("500"));
                Assert.That(results.Document, Is.Null);
                Assert.That(report.Requests.Count, Is.EqualTo(1));
                Assert.That(report.Requests[0].Validations.Any(v => v.Type == DiagnosticsValidationType.HttpError && v.Message.Contains("500")), Is.True);
            });
        }

        [Test]
        public async Task QueryEvents_DuplicateEventIDs()
        {
            var report = new DiagnosticsReport();
            var options = CreateQueryOptions();
            var epc = GetSampleEPC();
            var parameters = new EPCISQueryParameters(epc);
            string json = ReadTestData("EPCISQueryDocument_duplicate_eventIDs.jsonld");

            HttpClient client = BuildHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

            var results = await EPCISTraceabilityResolver.QueryEvents(options, parameters, client, report);

            Assert.Multiple(() =>
            {
                Assert.That(results.Errors.Count, Is.EqualTo(0));
                Assert.That(results.Document, Is.Not.Null);
                Assert.That(report.Requests.Count, Is.EqualTo(1));
                Assert.That(report.Requests[0].Validations.Any(v => v.Type == DiagnosticsValidationType.BusinessRuleError && v.Level == LogLevel.Error && v.Message.Contains("Duplicate event ID")), Is.True);
            });
        }

        [Test]
        public async Task QueryEvents_MalformedJSON()
        {
            var report = new DiagnosticsReport();
            var options = CreateQueryOptions();
            var epc = GetSampleEPC();
            var parameters = new EPCISQueryParameters(epc);
            string malformed = ReadTestData("EPCISQueryDocument_malformed.jsonld");

            HttpClient client = BuildHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(malformed, Encoding.UTF8, "application/json")
            });

            var results = await EPCISTraceabilityResolver.QueryEvents(options, parameters, client, report);

            Assert.Multiple(() =>
            {
                Assert.That(report.Requests.Count, Is.EqualTo(1));
                Assert.That(report.Requests[0].Validations.Any(v => v.Type == DiagnosticsValidationType.SchemaError && v.Level == LogLevel.Error), Is.True);
            });
        }

        [Test]
        public async Task Traceback_Success_NoCriticalErrors()
        {
            var report = new DiagnosticsReport();
            var options = CreateQueryOptions();
            string validJson = ReadTestData("EPCISQUERYDOCUMENT_advanced_1.jsonld");
            var doc = OpenTraceabilityMappers.EPCISQueryDocument.JSON.Map(validJson);
            var firstEvent = doc.Events.First();
            var epcForTrace = firstEvent.Products.FirstOrDefault()?.EPC ?? GetSampleEPC();

            HttpClient client = BuildHttpClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(validJson, Encoding.UTF8, "application/json")
            });

            var results = await EPCISTraceabilityResolver.Traceback(options, epcForTrace, client, null, report);

            Assert.Multiple(() =>
            {
                Assert.That(results.Errors.Count, Is.EqualTo(0));
                Assert.That(results.Document, Is.Not.Null);
                Assert.That(report.Validations.Any(v => v.Type == DiagnosticsValidationType.BusinessRuleError && v.Level == LogLevel.Error), Is.False);
            });
        }
    }
}