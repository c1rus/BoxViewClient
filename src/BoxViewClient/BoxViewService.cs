using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class BoxViewService : IDisposable
    {
        private HttpClient _httpClient;
        private Client _client;

        public BoxViewService(string apiKey)
        {
            _httpClient = new HttpClient();
            _client = new Client(_httpClient, apiKey);
        }

        public async Task<List<Document>> GetDocumentsAsync(int limit = 50)
        {
            var response = await _client.GetDocumentsAsync(new GetDocumentsRequest { Limit = limit });

            CheckIfRateLimitReached(response);

            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new BoxViewException(response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GetDocumentsResponse>(content);
            return result.DocumentCollection.Entries;
        }

        public async Task<Document> GetDocumentAsync(string documentId)
        {
            var response = await _client.GetDocumentAsync(new GetDocumentRequest { DocumentId = documentId });

            CheckIfRateLimitReached(response);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new BoxViewException(response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Document>(content);
            return result;
        }

        public async Task<Document> UploadDocumentFromUrlAsync(string url, string name = null, string thumbnails = null, bool nonSvg = false)
        {
            var response = await _client.UploadDocumentAsync(
                new UrlUploadDocumentRequest
                {
                    Url = new Uri(url),
                    Name = name ?? GetRandomName(),
                    NonSvg = nonSvg,
                    Thumbnails = thumbnails
                });

            CheckIfRateLimitReached(response);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                throw new BoxViewException(response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Document>(content);
            return result;
        }

        public async Task<Document> UploadDocumentFromStreamAsync(Stream stream, string filename, string name = null, string thumbnails = null, bool nonSvg = false)
        {
            var response = await _client.UploadDocumentAsync(
                new MultipartUploadDocumentRequest
                {
                    Filename = filename,
                    File = stream,
                    Name = name ?? GetRandomName(),
                    NonSvg = nonSvg,
                    Thumbnails = thumbnails
                });

            CheckIfRateLimitReached(response);

            if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                throw new BoxViewException(response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Document>(content);
            return result;
        }

        public async Task<bool> DeleteDocumentAsync(string documentId)
        {
            var response = await _client.DeleteDocumentAsync(new DeleteDocumentRequest { DocumentId = documentId });

            CheckIfRateLimitReached(response);

            return response.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public async Task<Session> CreateSessionAsync(string documentId, int? duration = null, DateTimeOffset? expiresAt = null, bool? isDownloadable = null, bool? isTextSelectable = null)
        {
            var response = await _client.CreateSessionAsync(new CreateSessionRequest
            {
                DocumentId = documentId,
                Duration = duration,
                ExpiresAt = expiresAt,
                IsDownloadable = isDownloadable,
                IsTextSelectable = isTextSelectable
            });

            CheckIfRateLimitReached(response);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                string content = await response.Content.ReadAsStringAsync();
                var session = JsonConvert.DeserializeObject<Session>(content);
                return session;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                return null;
            }
            else
            {
                throw new BoxViewException(response.StatusCode);
            }
        }

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            var response = await _client.DeleteSessionAsync(new DeleteSessionRequest { SessionId = sessionId });

            CheckIfRateLimitReached(response);

            return response.StatusCode == System.Net.HttpStatusCode.NoContent;
        }

        void CheckIfRateLimitReached(HttpResponseMessage response)
        {
            if ((int)response.StatusCode == 429)
            {
                IEnumerable<string> values = new List<string>();

                int seconds = 0;

                if(response.Headers.TryGetValues("Retry-After", out values))
                {
                    int.TryParse(values.FirstOrDefault(), out seconds);
                }

                throw new BoxViewRateLimitException(seconds);
            }
        }

        string GetRandomName()
        {
            return Guid.NewGuid().ToString("N");;
        }

        public void Dispose()
        {
            if(_client != null)
            {
                _client.Dispose();
            }
        }
    }
}
