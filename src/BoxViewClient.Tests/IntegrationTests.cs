using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BoxViewClient.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        //Warning! These are integration tests (not very good ones for that matter).
        //As such the TearDown attempts to clean up as much as possible (but still not enough).
        //Don't be a fool by pointing this at your production environment.
        //Don't complain when you did ;-) Here be dragons!

        //Complete your api key here.
        private const string ApiKey = "use-at-your-own-risk-api-key-here";
        private HttpClient _httpClient;
        private Client _client;

        private static readonly string RandomDocumentName = Guid.NewGuid().ToString("N");

        [SetUp]
        public void SetUp()
        {
            _httpClient = new HttpClient();
            _client = new Client(_httpClient, ApiKey);

            //Rate limiting - lovely workaround (still broken).
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        [Test]
        public async void GetDocuments()
        {
            var response = await _client.GetDocumentsAsync(new GetDocumentsRequest{ Limit = 50});
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeAnonymousType(
                content,
                new
                {
                    document_collection = new
                    {
                        total_count = 0,
                        entries = new []
                        {
                            new
                            {
                                type = "",
                                id = "",
                                status = "",
                                name = "",
                                created_at = ""
                            }
                        }
                    }
                });
        }

        [Test]
        public async void GetDocument()
        {
            var uploadResponse = (await _client.UploadDocumentAsync(
                new UrlUploadDocumentRequest
                {
                    Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                    Name = RandomDocumentName
                }));
            Assert.That(uploadResponse.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            var documentId = JsonConvert.DeserializeAnonymousType(await uploadResponse.Content.ReadAsStringAsync(),
                new
                {
                    id = ""
                }).id;
            var response = await _client.GetDocumentAsync(new GetDocumentRequest { DocumentId = documentId });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "GetDocument " + documentId + "failed.");
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeAnonymousType(
                content,
                new
                {
                    type = "",
                    id = "",
                    status = "",
                    name = "",
                    created_at = ""
                });
            Assert.That(actual.id, Has.Length.EqualTo(32));
            Assert.That(actual.type, Is.EqualTo("document"));
            Assert.That(actual.status, Is.EqualTo("queued").Or.EqualTo("processing").Or.EqualTo("done"));
            Assert.That(actual.name, Is.EqualTo(RandomDocumentName));
            Assert.That(actual.created_at, Is.Not.Empty);
        }

        [Test]
        public async void UploadDocumentFromUrl()
        {
            var response = await _client.UploadDocumentAsync(
                new UrlUploadDocumentRequest
                {
                    Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                    Name = RandomDocumentName,
                    NonSvg = false,
                    Thumbnails = "128x128"
                });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeAnonymousType(
                content,
                new
                {
                    type = "",
                    id = "",
                    status = "",
                    name = "",
                    created_at = ""
                });
            Assert.That(actual.id, Has.Length.EqualTo(32));
            Assert.That(actual.type, Is.EqualTo("document"));
            Assert.That(actual.status, Is.EqualTo("queued"));
            Assert.That(actual.name, Is.EqualTo(RandomDocumentName));
            Assert.That(actual.created_at, Is.Not.Empty);
        }

        [Test]
        public async void UploadDocumentFromStream()
        {
            using (var stream = typeof (IntegrationTests).
                Assembly.
                GetManifestResourceStream("BoxViewClient.Tests.file.xls"))
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                var response = await _client.UploadDocumentAsync(
                    new MultipartUploadDocumentRequest
                    {
                        Filename = "file.xls",
                        File = stream,
                        Name = RandomDocumentName,
                        NonSvg = false,
                        Thumbnails = "128x128"
                    });
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
                var actual = JsonConvert.DeserializeAnonymousType(
                    await response.Content.ReadAsStringAsync(),
                    new
                    {
                        type = "",
                        id = "",
                        status = "",
                        name = "",
                        created_at = ""
                    });
                Assert.That(actual.id, Has.Length.EqualTo(32));
                Assert.That(actual.type, Is.EqualTo("document"));
                Assert.That(actual.status, Is.EqualTo("queued"));
                Assert.That(actual.name, Is.EqualTo(RandomDocumentName));
                Assert.That(actual.created_at, Is.Not.Empty);
            }
        }

        [Test]
        public async void DeleteExistingDocument()
        {
            var documentId = JsonConvert.DeserializeAnonymousType(
                await (
                    await _client.UploadDocumentAsync(
                        new UrlUploadDocumentRequest
                        {
                            Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                            Name = RandomDocumentName
                        })).Content.ReadAsStringAsync(),
                new
                {
                    id = ""
                }).id;
            var response = await _client.DeleteDocumentAsync(new DeleteDocumentRequest {DocumentId = documentId});
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async void DeleteNonExistingDocument()
        {
            var response = await _client.DeleteDocumentAsync(new DeleteDocumentRequest { DocumentId = Guid.NewGuid().ToString("N") });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async void CreateSessionForNonExistingDocument()
        {
            var response = await _client.CreateSessionAsync(new CreateSessionRequest
            {
                DocumentId = Guid.NewGuid().ToString("N")
            });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async void CreateSessionForQueuedExistingDocument()
        {
            var documentId = JsonConvert.DeserializeAnonymousType(
                await (
                    await _client.UploadDocumentAsync(
                        new UrlUploadDocumentRequest
                        {
                            Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                            Name = RandomDocumentName
                        })).Content.ReadAsStringAsync(),
                new
                {
                    id = ""
                }).id;
            var response = await _client.CreateSessionAsync(new CreateSessionRequest
            {
                DocumentId = documentId
            });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        }

        [Test]
        public async void CreateSessionForExistingProcessedDocument()
        {
            var documentId = JsonConvert.DeserializeAnonymousType(
                await (
                    await _client.UploadDocumentAsync(
                        new UrlUploadDocumentRequest
                        {
                            Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                            Name = RandomDocumentName
                        })).Content.ReadAsStringAsync(),
                new
                {
                    id = ""
                }).id;

            //wait for it
            while (JsonConvert.DeserializeAnonymousType(
                await (
                    await _client.GetDocumentAsync(
                        new GetDocumentRequest
                        {
                            DocumentId = documentId
                        })).Content.ReadAsStringAsync(),
                new
                {
                    status = ""
                }).status != "done")
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var response = await _client.CreateSessionAsync(new CreateSessionRequest
            {
                DocumentId = documentId
            });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        }

        [Test]
        public async void DeleteExistingSession()
        {
            var documentId = JsonConvert.DeserializeAnonymousType(
               await (
                   await _client.UploadDocumentAsync(
                       new UrlUploadDocumentRequest
                       {
                           Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                           Name = RandomDocumentName
                       })).Content.ReadAsStringAsync(),
               new
               {
                   id = ""
               }).id;

            //wait for it
            var intermediateResponse = (
                await _client.GetDocumentAsync(
                    new GetDocumentRequest
                    {
                        DocumentId = documentId
                    }));
            var content = await intermediateResponse.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            while (JsonConvert.DeserializeAnonymousType(
                content,
                new
                {
                    status = ""
                }).status != "done")
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                intermediateResponse = (
                await _client.GetDocumentAsync(
                    new GetDocumentRequest
                    {
                        DocumentId = documentId
                    }));
                content = await intermediateResponse.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }

            //grab session
            var sessionId = JsonConvert.DeserializeAnonymousType(
                await (await _client.CreateSessionAsync(new CreateSessionRequest
                {
                    DocumentId = documentId
                })).Content.ReadAsStringAsync(),
                new
                {
                    id = ""
                
                }).id;

            var response = await _client.DeleteSessionAsync(new DeleteSessionRequest {SessionId = sessionId});
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async void DeleteNonExistingSession()
        {
            var response = await _client.DeleteSessionAsync(new DeleteSessionRequest { SessionId = Guid.NewGuid().ToString("N") });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [TearDown]
        public void TearDown()
        {
            Task.Run(async () =>
            {
                var response = await _client.GetDocumentsAsync(new GetDocumentsRequest { Limit = 50 });
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    foreach (var entry in JsonConvert.DeserializeAnonymousType(
                        content,
                        new
                        {
                            document_collection = new
                            {
                                total_count = 0,
                                entries = new[]
                                {
                                    new
                                    {
                                        type = "",
                                        id = "",
                                        status = "",
                                        name = "",
                                        created_at = ""
                                    }
                                }
                            }
                        }).document_collection.entries)
                    {
                        Guid _;
                        if (Guid.TryParse(entry.name, out _))
                        {
                            await _client.DeleteDocumentAsync(new DeleteDocumentRequest { DocumentId = entry.id });
                        }
                    }
                }
            }).Wait();
            _client.Dispose();
            _httpClient.Dispose();
        }
    }
}
