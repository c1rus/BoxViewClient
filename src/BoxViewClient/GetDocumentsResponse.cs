using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class GetDocumentsResponse
    {
        [JsonProperty("document_collection")]
        public DocumentCollection DocumentCollection { get; set; }
    }

    public class DocumentCollection 
    {
        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        public List<Document> Entries { get; set; }
    }
}