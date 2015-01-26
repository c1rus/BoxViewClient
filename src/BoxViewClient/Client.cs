using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class Client
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
        private readonly string _token;

        public Client(HttpClient client, string token)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (token == null) throw new ArgumentNullException("token");
            _client = client;
            _token = token;
        }

        public async Task<HttpResponseMessage> UploadDocumentAsync(UrlUploadDocument request)
        {
            if (request == null) 
                throw new ArgumentNullException("request");

            var content = new StringContent(JsonConvert.SerializeObject(request));
            content.Headers.Add("Authorization", string.Format("Token {0}", _token));
            content.Headers.ContentType.MediaType = "application/json";
            var response = await _client.
                PostAsync(
                    new Uri(ApiBaseUri, DocumentsUri),
                    content);
            while (429 == (int) response.StatusCode)
            {
            }
            return response;
        }

        public Task<HttpResponseMessage> UploadDocumentAsync(MultipartUploadDocument request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            using (var content =
                new MultipartFormDataContent(
                    string.Format("----Boundary{0}", Guid.NewGuid().ToString("N"))))
            {
                content.Headers.Add("Authorization", string.Format("Token {0}", _token));
                content.Headers.ContentType.MediaType = "multipart/form-data";
                content.Add(new StreamContent(request.File), "file");
                if (request.Name != null)
                {
                    content.Add(new StringContent(request.Name), "name");
                }
                if (request.Thumbnails != null)
                {
                    content.Add(new StringContent(string.Join(",", request.Thumbnails)), "thumbnails");
                }
                content.Add(new StringContent(
                    request.NonSvg
                        ? bool.TrueString.ToLowerInvariant()
                        : bool.FalseString.ToLowerInvariant()), "non_svg");
                return _client.
                    PostAsync(
                        new Uri(ApiBaseUri, DocumentsUri),
                        content);
            }
        }
    }
}
