using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Middlewares
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class PreventDuplicate
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMemoryCache _cache;

        public PreventDuplicate(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _cache = _serviceProvider.GetRequiredService<IMemoryCache>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Method == "POST")
            {
                StringValues key;
                if (httpContext.Request.Form.TryGetValue(Startup._formFieldName, out key))
                {
                    bool isUsed;
                    if (_cache.TryGetValue(key.ToString(), out isUsed))
                    {
                        httpContext.Abort();
                    }
                    _cache.Set(key.ToString(), true, TimeSpan.FromMinutes(5));
                }
            }


            await _next(httpContext);
        }
    }


}
