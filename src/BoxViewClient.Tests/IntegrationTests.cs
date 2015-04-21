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
            var actual = JsonConvert.DeserializeObject<GetDocumentsResponse>(content);
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
            var document = JsonConvert.DeserializeObject<Document>(await uploadResponse.Content.ReadAsStringAsync());

            var response = await _client.GetDocumentAsync(new GetDocumentRequest { DocumentId = document.Id });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "GetDocument " + document.Id + "failed.");
            var content = await response.Content.ReadAsStringAsync();
            var actual = JsonConvert.DeserializeObject<Document>(content);

            Assert.That(actual.Id, Has.Length.EqualTo(32));
            Assert.That(actual.Type, Is.EqualTo("document"));
            Assert.That(actual.StatusText, Is.EqualTo("queued").Or.EqualTo("processing").Or.EqualTo("done"));
            Assert.That(actual.Name, Is.EqualTo(RandomDocumentName));
            Assert.That(actual.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
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
            var actual = JsonConvert.DeserializeObject<Document>(content);
            Assert.That(actual.Id, Has.Length.EqualTo(32));
            Assert.That(actual.Type, Is.EqualTo("document"));
            Assert.That(actual.StatusText, Is.EqualTo("queued"));
            Assert.That(actual.Name, Is.EqualTo(RandomDocumentName));
            Assert.That(actual.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
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

                var content = await response.Content.ReadAsStringAsync();
                var actual = JsonConvert.DeserializeObject<Document>(content);

                Assert.That(actual.Id, Has.Length.EqualTo(32));
                Assert.That(actual.Type, Is.EqualTo("document"));
                Assert.That(actual.StatusText, Is.EqualTo("queued"));
                Assert.That(actual.Name, Is.EqualTo(RandomDocumentName));
                Assert.That(actual.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async void DeleteExistingDocument()
        {
            var document = JsonConvert.DeserializeObject<Document>(
                await (
                    await _client.UploadDocumentAsync(
                        new UrlUploadDocumentRequest
                        {
                            Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                            Name = RandomDocumentName
                        })).Content.ReadAsStringAsync());

            var response = await _client.DeleteDocumentAsync(new DeleteDocumentRequest { DocumentId = document.Id});
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
            var document = JsonConvert.DeserializeObject<Document>(
                await (
                    await _client.UploadDocumentAsync(
                        new UrlUploadDocumentRequest
                        {
                            Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                            Name = RandomDocumentName
                        })).Content.ReadAsStringAsync());

            var response = await _client.CreateSessionAsync(new CreateSessionRequest
            {
                DocumentId = document.Id
            });
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));
        }

        [Test]
        public async void CreateSessionForExistingProcessedDocument()
        {
            var document = JsonConvert.DeserializeObject<Document>(
                await (
                    await _client.UploadDocumentAsync(
                        new UrlUploadDocumentRequest
                        {
                            Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                            Name = RandomDocumentName
                        })).Content.ReadAsStringAsync());

            //wait for it
            while (JsonConvert.DeserializeObject<Document>(
                await (
                    await _client.GetDocumentAsync(
                        new GetDocumentRequest
                        {
                            DocumentId = document.Id
                        })).Content.ReadAsStringAsync()).Status != DocumentStatus.Done)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var response = await _client.CreateSessionAsync(new CreateSessionRequest
            {
                DocumentId = document.Id
            });

            string content = await response.Content.ReadAsStringAsync();

            var session = JsonConvert.DeserializeObject<Session>(content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(session.Type, Is.EqualTo("session"));
            Assert.That(session.Id, Is.Not.Null);
            Assert.That(session.ExpiresAt, Is.Not.EqualTo(DateTime.MinValue));
        }

        [Test]
        public async void DeleteExistingSession()
        {
            var document = JsonConvert.DeserializeObject<Document>(
               await (
                   await _client.UploadDocumentAsync(
                       new UrlUploadDocumentRequest
                       {
                           Url = new Uri("http://crypto.stanford.edu/DRM2002/darknet5.doc"),
                           Name = RandomDocumentName
                       })).Content.ReadAsStringAsync());

            //wait for it
            var intermediateResponse = (
                await _client.GetDocumentAsync(
                    new GetDocumentRequest
                    {
                        DocumentId = document.Id
                    }));

            var content = await intermediateResponse.Content.ReadAsStringAsync();
            Console.WriteLine(content);

            while (JsonConvert.DeserializeObject<Document>(content).Status != DocumentStatus.Done)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                intermediateResponse = (
                await _client.GetDocumentAsync(
                    new GetDocumentRequest
                    {
                        DocumentId = document.Id
                    }));
                content = await intermediateResponse.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }

            //grab session
            var session = JsonConvert.DeserializeObject<Session>(
                await (await _client.CreateSessionAsync(new CreateSessionRequest
                {
                    DocumentId = document.Id
                })).Content.ReadAsStringAsync());

            var response = await _client.DeleteSessionAsync(new DeleteSessionRequest {SessionId = session.Id});
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
                    foreach (var entry in JsonConvert.DeserializeObject<GetDocumentsResponse>(content).DocumentCollection.Entries)
                    {
                        Guid _;
                        if (Guid.TryParse(entry.Name, out _))
                        {
                            await _client.DeleteDocumentAsync(new DeleteDocumentRequest { DocumentId = entry.Id });
                        }
                    }
                }
            }).Wait();
            _client.Dispose();
            _httpClient.Dispose();
        }
    }
}
