using System;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class CreateSessionRequest
    {
        /// <summary>
        /// The ID of the document for which to create a session.
        /// </summary>
        [JsonProperty("document_id")]
        public string DocumentId { get; set; }
        /// <summary>
        /// The duration in minutes until the session expires (default=60).
        /// </summary>
        [JsonProperty("duration")]
        public int? Duration { get; set; }

        /// <summary>
        /// The timestamp at which the session should expire.
        /// </summary>
        [JsonProperty("expires_at")]
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// Whether a button will be shown allowing the user to download the original file. The original file will also be accessible via GET /sessions/{id}/content while the session is valid (default=false).
        /// </summary>
        [JsonProperty("is_downloadable")]
        public bool? IsDownloadable { get; set; }

        /// <summary>
        /// Whether text in the document will be selectable by the end user. This parameter will only affect the embedded iframe viewer. To disable text selection using viewer.js, use the enableTextSelection parameter (default=true).
        /// </summary>
        [JsonProperty("is_text_selectable")]
        public bool? IsTextSelectable { get; set; }
    }
}