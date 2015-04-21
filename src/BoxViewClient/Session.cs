using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BoxViewClient
{
    public class Session
    {
        /// <summary>
        /// session
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A unique string identifying this session.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The time when this session will expire
        /// </summary>
        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        ///  URLs for use with viewer.js provided as a convenience
        /// </summary>
        public SessionUrl Urls { get; set; }
    }

    public class SessionUrl
    {
        public string View { get; set; }

        public string Assets { get; set; }

        public string Realtime { get; set; }
    }
}
