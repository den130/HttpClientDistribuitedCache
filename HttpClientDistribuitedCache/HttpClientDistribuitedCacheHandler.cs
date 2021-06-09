using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HttpClientDistribuitedCache.Config;
using HttpClientDistribuitedCache.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HttpClientDistribuitedCache
{
    public class HttpClientDistribuitedCacheHandler : DelegatingHandler
    {
        private readonly ILogger<HttpClientDistribuitedCacheHandler> logger;
        private readonly IDistributedCache distribuiteCache;
        private readonly HttpClientDistribuitedCacheConfig config;

        public HttpClientDistribuitedCacheHandler(
            ILogger<HttpClientDistribuitedCacheHandler> logger,
            IDistributedCache distribuiteCache,
            IOptions<HttpClientDistribuitedCacheConfig> options)
        {
            this.logger = logger;
            this.distribuiteCache = distribuiteCache;
            this.config = options.Value;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var cacheRule = FindCacheRule(request);

                if (cacheRule != null)
                {
                    var cachedData = await distribuiteCache.GetStringAsync(ResponseKey(request), cancellationToken);

                    if (!string.IsNullOrEmpty(cachedData))
                    {
                        var cachedResponse = request.CashedHttpResponseMessage(cachedData);
                        return cachedResponse;
                    }
                }

                // base.SendAsync calls the inner handler
                var response = await base.SendAsync(request, cancellationToken);

                if (cacheRule != null)
                {
                    var newCachedResponse = await response.ToChachedResponse();
                    var options = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(DateTime.Now + this.config.DefaultAbsoluteExpiration)
                        .SetSlidingExpiration(this.config.DefaultSlidingExpiration);

                    if (cacheRule.UseCacheControlHeaderIfExists && (response.Headers.CacheControl?.MaxAge.HasValue ?? false))
                    {
                        options.SetAbsoluteExpiration(DateTime.Now + response.Headers.CacheControl.MaxAge.Value);
                    }

                    await distribuiteCache.SetStringAsync(ResponseKey(request), JsonConvert.SerializeObject(newCachedResponse), options, cancellationToken);
                }

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to get response");
                throw;
            }
        }

        private static string ResponseKey(HttpRequestMessage request)
        {
            return $"{request.Method}_{request.RequestUri.AbsolutePath}";
        }

        private RequestCacheRuleConfig FindCacheRule(HttpRequestMessage request)
        {
            return this.config.RequestsCacheRules.FirstOrDefault(x =>
                            Regex.IsMatch(request.RequestUri.AbsolutePath, x.RegexPath)
                            && string.Equals(request.Method.Method, x.HttpMethod, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
