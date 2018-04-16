using System;
using System.Net;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EMG.Lambda.LocalRunner.Internal
{
    public class InnerFunctionRunnerBuilder<TFunction> : IFunctionRunnerBuilder<TFunction>
        where TFunction : class, new()
    {
        public int Port { get; set; }

        public Func<ILambdaSerializer> SerializerFactory { get; set; }

        private readonly RequestDelegate _requestHandler;

        public InnerFunctionRunnerBuilder(RequestDelegate requestHandler)
        {
            _requestHandler = requestHandler ?? throw new ArgumentNullException(nameof(requestHandler));
        }

        public IRunner Build()
        {
            IWebHostBuilder builder = new WebHostBuilder();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<TFunction>();
                services.AddSingleton(sp => SerializerFactory());
            });

            builder.Configure(app => { app.Run(_requestHandler); });

            builder.UseKestrel(options => { options.Listen(IPAddress.Loopback, Port); });

            var host = builder.Build();

            return new LambdaRunner(host);
        }
    }
}