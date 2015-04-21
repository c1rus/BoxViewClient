using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class Document
    {
        /// <summary>
        /// document
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A unique string identifying this document.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///  An enum indicating the conversion status of this document. Can be queued, processing, done or error
        /// </summary>
        public DocumentStatus Status
        {
            get
            {
                switch (StatusText)
                {
                    case "queued":
                        return DocumentStatus.Queued;
                    case "processing":
                        return DocumentStatus.Processing;
                    case "done":
                        return DocumentStatus.Done;
                    case "error":
                        return DocumentStatus.Error;
                    default:
                        return DocumentStatus.None;
                }
            }
        }

        [JsonProperty("status")]
        public string StatusText { get; set; }

        /// <summary>
        ///  The name of this document.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///   The time the document was uploaded
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public enum DocumentStatus
    {
        None,
        Queued,
        Processing,
        Done,
        Error
    }
}
