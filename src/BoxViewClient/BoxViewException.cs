using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BoxViewClient
{
    public class BoxViewException : Exception
    {
        public HttpStatusCode Status { get; set; }

        public BoxViewException(HttpStatusCode status)
        {
            Status = status;
        }

        public BoxViewException(string message)
            : base(message)
        {
        }

        public BoxViewException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class BoxViewRateLimitException : Exception
    {
        public int Seconds { get; set; }

        public BoxViewRateLimitException(int seconds)
        {
            Seconds = seconds;
        }

        public BoxViewRateLimitException(string message)
            : base(message)
        {
        }

        public BoxViewRateLimitException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
