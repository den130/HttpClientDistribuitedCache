# HttpClientDistribuitedCache

## Startup.cs example
```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddHttpClientDistribuitedCacheExtensionsServices(this.Configuration);

    services.AddHttpClient("externalservice", c =>
    {
        c.BaseAddress = new Uri("https://localhost:5001/");
    })
    .AddHttpMessageHandler<HttpClientCacheHandler>();

    // Add a IDistribuitedCache implementation service, for example Redis
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = _config["MyRedisConStr"];
        options.InstanceName = "SampleInstance";
    });
    

    // Add a IDistribuitedCache implementation service, for example Memory cache
    // services.AddDistributedMemoryCache();

    ...
}

```

## appsettings.json example
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "HttpClientDistribuitedCache": {
    "DefaultAbsoluteExpiration": "00:00:15",
    "DefaultSlidingExpiration": "00:00:03",
    "RequestsCacheRules": [
      {
        "HttpMethod": "Get",
        "RegexPath": "^/api/v1/item/",
        "UseCacheControlHeaderIfExists": true
      },
      {
        "HttpMethod": "Get",
        "RegexPath": "^/api/v1/user/",
        "UseCacheControlHeaderIfExists": true
      }
    ]
  }
```

## Links

https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0
https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-5.0#distributed-redis-cache
