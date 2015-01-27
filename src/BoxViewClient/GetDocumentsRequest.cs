using System;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class GetDocumentsRequest
    {
        /// <summary>
        /// The number of documents to return (default=10, max=50).
        /// </summary>
        [JsonProperty("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// An upper limit on the creation timestamps of documents returned (default=now).
        /// </summary>
        [JsonProperty("created_before")]
        public DateTimeOffset? CreatedBefore { get; set; }

        /// <summary>
        /// A lower limit on the creation timestamps of documents returned.
        /// </summary>
        [JsonProperty("created_after")]
        public DateTimeOffset? CreatedAfter { get; set; }
    }
}