using System.Threading.Tasks;
using Amazon.Lambda.Serialization.Json;
using EMG.Lambda.LocalRunner;
using TemplatedLambda;

namespace TemplatedLambdaTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await LambdaRunner.Create()
                             .UsePort(5001)
                             .Receives<string>()
                             .UsesAsyncFunctionWithNoResult<Function>((function, input, context) => function.FunctionHandlerAsync(input, context))
                             .Build()
                             .StartAndWaitAsync();
        }
    }
}
