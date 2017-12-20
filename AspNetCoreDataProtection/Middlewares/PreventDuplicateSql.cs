using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Middlewares
{
    public class PreventDuplicateSql
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDistributedCache _cache;

        public PreventDuplicateSql(RequestDelegate next, IDistributedCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Method == "POST")
            {
                StringValues key;
                if (httpContext.Request.Form.TryGetValue(Startup._formFieldName, out key))
                {
                    var value = await _cache.GetAsync(key);
                    if (value != null)
                    {
                        httpContext.Abort();
                    }
                    else
                    {
                        value = Encoding.UTF8.GetBytes(key);
                        var cacheEntryOptions = new DistributedCacheEntryOptions()
                                                   .SetSlidingExpiration(TimeSpan.FromMinutes(3));
                        _cache.Set(key, value, cacheEntryOptions);
                    }

                }
            }
            await _next(httpContext);
        }
    }
}
