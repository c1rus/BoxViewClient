using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace BoxViewClient.Tests
{
    [TestFixture]
    public class BoxServiceTests
    {
        private const string ApiKey = "use-at-your-own-risk-api-key-here";
        private BoxViewService _boxViewService;
        private static readonly string RandomDocumentName = Guid.NewGuid().ToString("N");
        private const string fileUrl = "http://crypto.stanford.edu/DRM2002/darknet5.doc";

        [SetUp]
        public void SetUp()
        {
            _boxViewService = new BoxViewService(ApiKey);

            //Rate limiting - lovely workaround (still broken).
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        [Test]
        public async void GetDocuments()
        {
            var documents = await _boxViewService.GetDocumentsAsync();
        }

        [Test]
        public async void GetDocument()
        {
            var uploadDocument = await _boxViewService.UploadDocumentFromUrlAsync(fileUrl, RandomDocumentName);

            var document = await _boxViewService.GetDocumentAsync(uploadDocument.Id);

            Assert.That(document.Id, Has.Length.EqualTo(32));
            Assert.That(document.Type, Is.EqualTo("document"));
            Assert.That(document.StatusText, Is.EqualTo("queued").Or.EqualTo("processing").Or.EqualTo("done"));
            Assert.That(document.Name, Is.EqualTo(RandomDocumentName));
            Assert.That(document.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        }

        [Test]
        public async void UploadDocumentFromUrl()
        {
            var document = await _boxViewService.UploadDocumentFromUrlAsync(fileUrl, RandomDocumentName, "128x128");

            Assert.That(document.Id, Has.Length.EqualTo(32));
            Assert.That(document.Type, Is.EqualTo("document"));
            Assert.That(document.StatusText, Is.EqualTo("queued").Or.EqualTo("processing").Or.EqualTo("done"));
            Assert.That(document.Name, Is.EqualTo(RandomDocumentName));
            Assert.That(document.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
        }

        [Test]
        public async void UploadDocumentFromStream()
        {
            using (var stream = typeof(IntegrationTests).
                Assembly.
                GetManifestResourceStream("BoxViewClient.Tests.file.xls"))
            {
                await Task.Delay(TimeSpan.FromSeconds(2));

                var document = await _boxViewService.UploadDocumentFromStreamAsync(stream, "file.xls", RandomDocumentName, "128x128");

                Assert.That(document.Id, Has.Length.EqualTo(32));
                Assert.That(document.Type, Is.EqualTo("document"));
                Assert.That(document.StatusText, Is.EqualTo("queued"));
                Assert.That(document.Name, Is.EqualTo(RandomDocumentName));
                Assert.That(document.CreatedAt, Is.Not.EqualTo(DateTime.MinValue));
            }
        }

        [Test]
        public async void DeleteExistingDocument()
        {
            var uploadDocument = await _boxViewService.UploadDocumentFromUrlAsync(fileUrl, RandomDocumentName);

            var result = await _boxViewService.DeleteDocumentAsync(uploadDocument.Id);
            Assert.That(result, Is.True);
        }

        [Test]
        public async void DeleteNonExistingDocument()
        {
            var result = await _boxViewService.DeleteDocumentAsync(Guid.NewGuid().ToString("N"));
            Assert.That(result, Is.False);
        }

        [Test]
        [ExpectedException(typeof(BoxViewException))]
        public async void CreateSessionForNonExistingDocument()
        {
            var session = await _boxViewService.CreateSessionAsync(Guid.NewGuid().ToString("N"));
        }

        [Test]
        public async void CreateSessionForQueuedExistingDocument()
        {
            var document = await _boxViewService.UploadDocumentFromUrlAsync(fileUrl, RandomDocumentName);

            var session = await _boxViewService.CreateSessionAsync(document.Id);

            Assert.That(session, Is.Null);
        }

        [Test]
        public async void CreateSessionForExistingProcessedDocument()
        {
            var document = await _boxViewService.UploadDocumentFromUrlAsync(fileUrl, RandomDocumentName);

            //wait for it
            while ((await _boxViewService.GetDocumentAsync(document.Id)).Status != DocumentStatus.Done)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var session = await _boxViewService.CreateSessionAsync(document.Id);

            Assert.That(session.Type, Is.EqualTo("session"));
            Assert.That(session.Id, Is.Not.Null);
            Assert.That(session.ExpiresAt, Is.Not.EqualTo(DateTime.MinValue));
        }

        [Test]
        public async void DeleteExistingSession()
        {
            var document = await _boxViewService.UploadDocumentFromUrlAsync(fileUrl, RandomDocumentName);

            //wait for it
            while ((await _boxViewService.GetDocumentAsync(document.Id)).Status != DocumentStatus.Done)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            var session = await _boxViewService.CreateSessionAsync(document.Id);

            var result = await _boxViewService.DeleteSessionAsync(session.Id);

            Assert.That(result, Is.True);
        }

        [Test]
        public async void DeleteNonExistingSession()
        {
            var result = await _boxViewService.DeleteSessionAsync(Guid.NewGuid().ToString("N"));
            Assert.That(result, Is.False);
        }

        [TearDown]
        public void TearDown()
        {
            Task.Run(async () =>
            {
                var documents = await _boxViewService.GetDocumentsAsync();
                foreach (var doc in documents)
                {
                    await _boxViewService.DeleteDocumentAsync(doc.Id);           
                }
            }).Wait();

            _boxViewService.Dispose();
        }
    }
}
