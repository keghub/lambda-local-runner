using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EMG.Lambda.LocalRunner.Internal
{
    public class InnerReceivingRunnerBuilder<TInput> : IReceivingRunnerBuilder<TInput>
    {
        public int Port { get; set; }

        public Func<ILambdaSerializer> SerializerFactory { get; set; }

        public Func<ILambdaContext> LambdaContextFactory { get; set; }

        public IReturningRunnerBuilder<TInput, TOutput> Returns<TOutput>()
        {
            return new InnerReturningRunnerBuilder<TInput, TOutput>
            {
                Port = Port,
                SerializerFactory = SerializerFactory,
                LambdaContextFactory = LambdaContextFactory
            };
        }

        public IFunctionRunnerBuilder<TFunction> UsesAsyncFunctionWithNoResult<TFunction>(Func<TFunction, TInput, ILambdaContext, Task> executor)
            where TFunction : class, new()
        {
            return new InnerFunctionRunnerBuilder<TFunction>(Handler)
            {
                Port = Port,
                SerializerFactory = SerializerFactory
            };

            async Task Handler(HttpContext context)
            {
                var function = context.RequestServices.GetRequiredService<TFunction>();

                var serializer = context.RequestServices.GetRequiredService<ILambdaSerializer>();

                var lambdaContext = LambdaContextFactory();

                var input = serializer.Deserialize<TInput>(context.Request.Body);

                var stopwatch = Stopwatch.StartNew();

                await executor(function, input, lambdaContext);

                stopwatch.Stop();

                LogExecution(input, stopwatch.Elapsed);
            }
        }

        public IFunctionRunnerBuilder<TFunction> UsesFunctionWithNoResult<TFunction>(Action<TFunction, TInput, ILambdaContext> executor)
            where TFunction : class, new()
        {
            return new InnerFunctionRunnerBuilder<TFunction>(Handler)
            {
                Port = Port,
                SerializerFactory = SerializerFactory
            };

            Task Handler(HttpContext context)
            {
                var function = context.RequestServices.GetRequiredService<TFunction>();

                var serializer = context.RequestServices.GetRequiredService<ILambdaSerializer>();

                var lambdaContext = LambdaContextFactory();

                var input = serializer.Deserialize<TInput>(context.Request.Body);

                var stopwatch = Stopwatch.StartNew();

                executor(function, input, lambdaContext);

                stopwatch.Stop();

                LogExecution(input, stopwatch.Elapsed);

                return Task.CompletedTask;
            }
        }

        private static void LogExecution(TInput input, TimeSpan elapsed)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Received {input}");
            sb.AppendLine($"\tElapsed: {elapsed.TotalMilliseconds} ms");
            sb.AppendLine();

            Console.Write(sb.ToString());
        }
    }
}