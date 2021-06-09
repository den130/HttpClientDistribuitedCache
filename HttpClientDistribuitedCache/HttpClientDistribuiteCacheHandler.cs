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

    public class HttpClientCacheHandler : DelegatingHandler
    {
        private readonly ILogger<HttpClientCacheHandler> logger;
        private readonly IDistributedCache distribuiteCache;
        private readonly HttpClientDistribuitedCacheConfig config;

        public HttpClientCacheHandler(
            ILogger<HttpClientCacheHandler> logger,
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
            logger.LogInformation($"Request: {request}");
            try
            {
                if (this.config.RequestsCacheRules.Any(x => 
                Regex.IsMatch(request.RequestUri.AbsolutePath, x.RegexPath)
                && string.Equals(request.Method.Method, x.HttpMethod, StringComparison.InvariantCultureIgnoreCase)))
                    {

                    var cachedData = await distribuiteCache.GetStringAsync($"{request.Method}_{request.RequestUri.AbsolutePath}", cancellationToken);

                    if (!string.IsNullOrEmpty(cachedData))
                    {
                        return request.CashedHttpResponseMessage(cachedData);
                    }
                }

                // base.SendAsync calls the inner handler
                var response = await base.SendAsync(request, cancellationToken);

                var newCachedResponse = await response.ToChachedResponse();
                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(DateTime.Now + this.config.DefaultAbsoluteExpiration)
                    .SetSlidingExpiration(this.config.DefaultSlidingExpiration);

                await distribuiteCache.SetStringAsync($"{request.Method}_{request.RequestUri.AbsolutePath}", JsonConvert.SerializeObject(newCachedResponse), options);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to get response: {ex}");
                throw;
            }
        }
    }
}
