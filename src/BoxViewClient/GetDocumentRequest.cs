using Newtonsoft.Json;

namespace BoxViewClient
{
    public class GetDocumentRequest
    {
        /// <summary>
        /// The ID of the document to fetch.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// Comma-separated list of fields to return. id and type are always returned.
        /// </summary>
        [JsonProperty("fields")]
        public string Fields { get; set; }
    }
}