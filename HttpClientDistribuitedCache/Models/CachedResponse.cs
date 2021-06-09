using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientDistribuitedCache.Models
{
    public class CachedResponse
    {
        public HttpStatusCode StatusCoode { get; set; }

        public string ReasonPhrase { get; set; }

        public Version Version { get; set; }

        public byte[] Content { get; set; }

        public IDictionary<string, IEnumerable<string>> Headers { get; set; }

        public IDictionary<string, IEnumerable<string>> ContentHeaders { get; set; }
    }
}
