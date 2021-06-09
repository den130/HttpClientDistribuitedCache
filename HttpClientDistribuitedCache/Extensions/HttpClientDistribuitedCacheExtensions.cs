using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HttpClientDistribuitedCache.Config;
using HttpClientDistribuitedCache.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace HttpClientDistribuitedCache.Extensions
{
    public static class HttpClientDistribuitedCacheExtensions
    {
        public static IServiceCollection AddHttpClientDistribuitedCacheExtensionsServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<HttpClientDistribuitedCacheConfig>(configuration.GetSection(HttpClientDistribuitedCacheConfig.SectionName));

            services.AddTransient<HttpClientDistribuitedCacheHandler>();

            return services;
        }

        public static async Task<CachedResponse> ToChachedResponse(this HttpResponseMessage response)
        {
            var cachedResponse = new CachedResponse
            {
                Version = response.Version,
                ContentHeaders = response.Content.Headers.Where(h => h.Value != null && h.Value.Any()).ToDictionary(h => h.Key, h => h.Value),
                Headers = response.Headers.Where(h => h.Value != null && h.Value.Any()).ToDictionary(h => h.Key, h => h.Value),
                ReasonPhrase = response.ReasonPhrase,
                StatusCoode = response.StatusCode,
            };

            cachedResponse.Content = await response.Content.ReadAsByteArrayAsync();

            return cachedResponse;
        }

        public static HttpResponseMessage CashedHttpResponseMessage(this HttpRequestMessage request, string cachedData)
        {
            var cachedResponse = JsonConvert.DeserializeObject<CachedResponse>(cachedData);
            var httpCachedResponse = new HttpResponseMessage(cachedResponse.StatusCoode);
            httpCachedResponse.Content = new ByteArrayContent(cachedResponse.Content);
            httpCachedResponse.ReasonPhrase = cachedResponse.ReasonPhrase;
            httpCachedResponse.Version = cachedResponse.Version;
            if (cachedResponse.Headers != null)
            {
                foreach (var kvp in cachedResponse.Headers)
                {
                    httpCachedResponse.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            if (cachedResponse.ContentHeaders != null)
            {
                foreach (var kvp in cachedResponse.ContentHeaders)
                {
                    httpCachedResponse.Content.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            httpCachedResponse.RequestMessage = request;
            return httpCachedResponse;
        }
    }
}
