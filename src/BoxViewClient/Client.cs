using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class Client : IDisposable
    {
        private static readonly Uri ApiBaseUri = new Uri(
            "https://view-api.box.com/1/");
        private static readonly Uri UploadBaseUri = new Uri(
            "https://upload.view-api.box.com/1/");

        private static readonly Uri DocumentsUri = new Uri(
            "documents", UriKind.Relative);

        private static readonly Uri SessionsUri = new Uri(
            "sessions", UriKind.Relative);

        private readonly HttpClient _client;
        private readonly JsonSerializerSettings _settings;

        public Client(HttpClient client, string apiKey)
        {
            if (client == null) 
                throw new ArgumentNullException("client");
            if (apiKey == null) 
                throw new ArgumentNullException("apiKey");
            _client = client;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", apiKey);
            _settings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        public Task<HttpResponseMessage> GetDocumentsAsync(GetDocumentsRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var builder = new UriBuilder(new Uri(ApiBaseUri, DocumentsUri));
            if (request.Limit.HasValue)
            {
                builder.Query += (builder.Query.Length != 0 ? "&" : "") + 
                    string.Format("limit={0}", request.Limit.Value);
            }
            if (request.CreatedBefore.HasValue)
            {
                builder.Query += (builder.Query.Length != 0 ? "&" : "") + 
                    string.Format("created_before={0}", XmlConvert.ToString(request.CreatedBefore.Value));
            }
            if (request.CreatedAfter.HasValue)
            {
                builder.Query += (builder.Query.Length != 0 ? "&" : "") + 
                    string.Format("created_after={0}", XmlConvert.ToString(request.CreatedAfter.Value));
            }
            return _client.GetAsync(builder.Uri);
        }

        public Task<HttpResponseMessage> GetDocumentAsync(GetDocumentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var builder = new UriBuilder(new Uri(ApiBaseUri, DocumentsUri));
            builder.Path += (builder.Path.EndsWith("/") ? "" : "/") +
                            request.DocumentId;
            builder.Query += string.Format("fields={0}", request.Fields);
            return _client.GetAsync(builder.Uri);
        }

        public Task<HttpResponseMessage> UploadDocumentAsync(UrlUploadDocumentRequest request)
        {
            if (request == null) 
                throw new ArgumentNullException("request");

            var content = new StringContent(JsonConvert.SerializeObject(request, _settings), null, "application/json");
            return _client.
                PostAsync(
                    new Uri(ApiBaseUri, DocumentsUri),
                    content);
        }

        public Task<HttpResponseMessage> UploadDocumentAsync(MultipartUploadDocumentRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var content =
                new MultipartFormDataContent(
                    string.Format("----Boundary{0}", Guid.NewGuid().ToString("N")))
                {
                    {
                        new StreamContent(request.File), "file", request.Filename
                    }
                };
            if (request.Name != null)
            {
                content.Add(new StringContent(request.Name), "name");
            }
            if (request.Thumbnails != null)
            {
                content.Add(new StringContent(request.Thumbnails), "thumbnails");
            }
            if (request.NonSvg.HasValue)
            {
                content.Add(new StringContent(
                    request.NonSvg.Value
                        ? bool.TrueString.ToLowerInvariant()
                        : bool.FalseString.ToLowerInvariant()), "non_svg");
            }
            return _client.
                PostAsync(
                    new Uri(UploadBaseUri, DocumentsUri),
                    content);
            
        }

        public Task<HttpResponseMessage> DeleteDocumentAsync(DeleteDocumentRequest request)
        {
            if (request == null) 
                throw new ArgumentNullException("request");

            var builder = new UriBuilder(new Uri(ApiBaseUri, DocumentsUri));
            builder.Path += builder.Path.EndsWith("/")
                ? request.DocumentId
                : "/" + request.DocumentId;
            return _client.DeleteAsync(builder.Uri);
        }

        public Task<HttpResponseMessage> CreateSessionAsync(CreateSessionRequest request)
        {
            if (request == null) 
                throw new ArgumentNullException("request");

            var content = new StringContent(JsonConvert.SerializeObject(request, _settings), null, "application/json");
            return _client.
                PostAsync(
                    new Uri(ApiBaseUri, SessionsUri),
                    content);
        }

        public Task<HttpResponseMessage> DeleteSessionAsync(DeleteSessionRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var builder = new UriBuilder(new Uri(ApiBaseUri, SessionsUri));
            builder.Path += builder.Path.EndsWith("/")
                ? request.SessionId
                : "/" + request.SessionId;
            return _client.DeleteAsync(builder.Uri);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
