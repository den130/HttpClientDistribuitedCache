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
