using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Container.Fallback.Web
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestServices(DefineServices().BuildServiceProvider());

            app.UseMiddleware(typeof(MyMiddleware));
            app.UseMiddleware(typeof(MyMiddleware));

            app.Run(async context =>
                await context.Response.WriteAsync("---------- Done\r\n"));
        }

        public IEnumerable<IServiceDescriptor> DefineServices()
        {
            var describer = new ServiceDescriber();
            yield return describer.Singleton<ICall, CallOne>();
            yield return describer.Scoped<ICall, CallTwo>();
            yield return describer.Transient<ICall, CallThree>();

            yield return describer.Transient<ITypeActivator, TypeActivator>();
            yield return describer.Describe(
                typeof(IScopedInstance<>),
                typeof(ScopedInstance<>),
                lifecycle: LifecycleKind.Scoped);
        }
    }

    public class MyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnumerable<ICall> _calls;

        public MyMiddleware(RequestDelegate next, IEnumerable<ICall> calls)
        {
            _next = next;
            _calls = calls;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            await context.Response.WriteAsync("---------- MyMiddleware ctor\r\n");
            await context.Response.WriteAsync(_calls.Aggregate("", (a, b) => a + b.Text + "\r\n"));

            var requestContainerCalls = context.RequestServices.GetService<IEnumerable<ICall>>();
            await context.Response.WriteAsync("---------- context.RequestServices\r\n");
            await context.Response.WriteAsync(requestContainerCalls.Aggregate("", (a, b) => a + b.Text + "\r\n"));

            await _next(context);
        }
    }

    public interface ICall
    {
        string Text { get; }
    }

    public abstract class CallBase : ICall
    {
        private static int _lastNumber;
        private readonly int _number;

        public CallBase()
        {
            _number = Interlocked.Increment(ref _lastNumber);
        }

        public string Text
        {
            get { return GetType().Name + "[" + _number + "]"; }
        }
    }

    public class CallOne : CallBase
    {
    }

    public class CallTwo : CallBase
    {
    }

    public class CallThree : CallBase
    {
    }
}
