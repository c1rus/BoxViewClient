using System;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class UrlUploadDocumentRequest
    {
        /// <summary>
        /// The URL of the document to be converted.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }

        /// <summary>
        /// The name of this document.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Comma-separated list of thumbnail dimensions of the format {width}x{height} e.g. 128×128,256×256 – width can be between 16 and 1024, height between 16 and 768.
        /// </summary>
        [JsonProperty("thumbnails")]
        public string Thumbnails { get; set; }

        /// <summary>
        /// Whether to also create the non-svg version of the document (default=false).
        /// </summary>
        [JsonProperty("non_svg")]
        public bool? NonSvg { get; set; }
    }
}