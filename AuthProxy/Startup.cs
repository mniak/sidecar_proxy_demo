using AuthProxy.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using Yarp.ReverseProxy.Forwarder;

namespace AuthProxy
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ProxyOptions>(Configuration.GetSection("Proxy"));
            services.AddSingleton(svc => svc.GetRequiredService<IOptions<ProxyOptions>>().Value);
            services.AddHttpForwarder();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHttpForwarder forwarder,
            ProxyOptions proxyOptions)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var invoker = new HttpMessageInvoker(new SocketsHttpHandler()
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false,
            });
            var transformer = new ProxyTransformer();
            var requestOptions = new ForwarderRequestConfig { Timeout = TimeSpan.FromSeconds(100) };

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/{**catch-all}", async httpContext =>
                {
                    var error = await forwarder.SendAsync(httpContext, proxyOptions.Upstream, invoker, requestOptions, transformer);
                    if (error != ForwarderError.None)
                    {
                        var errorFeature = httpContext.Features.Get<IForwarderErrorFeature>();
                        var exception = errorFeature.Exception;
                    }
                });
            });
        }
    }
}
