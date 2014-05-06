// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;

namespace Builder.Middleware.Web
{
    public class Startup
    {
        public void Configuration(IBuilder app)
        {
            app.UseXHttpHeaderOverride();
            app.UseMiddleware(typeof(MyMiddleware), "Yo");
        }
    }

    public static class BuilderExtensions
    {
        public static IBuilder UseXHttpHeaderOverride(this IBuilder builder)
        {
            return builder.UseMiddleware(typeof(XHttpHeaderOverrideMiddleware));
        }
    }

    public class XHttpHeaderOverrideMiddleware
    {
        private readonly RequestDelegate _next;

        public XHttpHeaderOverrideMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var headerValue = httpContext.Request.Headers["X-HTTP-Method-Override"];
            var queryValue = httpContext.Request.Query["X-HTTP-Method-Override"];

            if (!string.IsNullOrEmpty(headerValue))
            {
                httpContext.Request.Method = headerValue;
            }
            else if (!string.IsNullOrEmpty(queryValue))
            {
                httpContext.Request.Method = queryValue;
            }

            return _next.Invoke(httpContext);
        }
    }

    public class MyMiddleware
    {
        private RequestDelegate _next;
        private string _greeting;
        private IServiceProvider _services;

        public MyMiddleware(RequestDelegate next, string greeting, IServiceProvider services)
        {
            _next = next;
            _greeting = greeting;
            _services = services;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await httpContext.Response.WriteAsync(_greeting + ", middleware!\r\n");
            await httpContext.Response.WriteAsync("This request is a " + httpContext.Request.Method + "\r\n");
        }
    }
}
