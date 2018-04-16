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
    public class InnerReturningRunnerBuilder<TInput, TOutput> : IReturningRunnerBuilder<TInput, TOutput>
    {
        public int Port { get; set; }

        public Func<ILambdaSerializer> SerializerFactory { get; set; }


        public IFunctionRunnerBuilder<TFunction> UsesAsyncFunction<TFunction>(Func<TFunction, TInput, ILambdaContext, Task<TOutput>> executor)
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

                var lambdaContext = new TestLambdaContext();

                var input = serializer.Deserialize<TInput>(context.Request.Body);

                var stopwatch = Stopwatch.StartNew();

                var output = await executor(function, input, lambdaContext);

                stopwatch.Stop();

                serializer.Serialize(output, context.Response.Body);

                LogExecution(input, output, stopwatch.Elapsed);
            }
        }

        public IFunctionRunnerBuilder<TFunction> UsesFunction<TFunction>(Func<TFunction, TInput, ILambdaContext, TOutput> executor)
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

                var lambdaContext = new TestLambdaContext();

                var input = serializer.Deserialize<TInput>(context.Request.Body);

                var stopwatch = Stopwatch.StartNew();

                var output = executor(function, input, lambdaContext);

                stopwatch.Stop();

                serializer.Serialize(output, context.Response.Body);

                LogExecution(input, output, stopwatch.Elapsed);

                return Task.CompletedTask;
            }
        }

        private static void LogExecution(TInput input, TOutput output, TimeSpan elapsed)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Received {input}");
            sb.AppendLine($"\tProduced {output}");
            sb.AppendLine($"\tElapsed: {elapsed.TotalMilliseconds} ms");
            sb.AppendLine();

            Console.Write(sb.ToString());
        }
    }
}