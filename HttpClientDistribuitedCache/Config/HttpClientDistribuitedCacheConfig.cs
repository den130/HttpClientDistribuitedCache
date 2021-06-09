using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpClientDistribuitedCache.Config
{
    public class HttpClientDistribuitedCacheConfig
    {
        public const string SectionName = "HttpClientDistribuitedCache";

        public TimeSpan DefaultAbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(10);

        public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(2);

        public IEnumerable<RequestCacheRuleConfig> RequestsCacheRules { get; set; }
    }
}
