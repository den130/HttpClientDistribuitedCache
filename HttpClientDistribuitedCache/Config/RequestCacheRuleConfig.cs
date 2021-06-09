using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientDistribuitedCache.Config
{
    public class RequestCacheRuleConfig
    {
        public string HttpMethod { get; set; }

        public string RegexPath { get; set; }

        public bool UseCacheControlHeaderIfExists { get; set; } = false;
    }
}
