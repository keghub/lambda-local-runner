using System.Threading.Tasks;
using Amazon.Lambda.Serialization.Json;
using EMG.Lambda.LocalRunner;
using Lambda;

namespace LambdaTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await LambdaRunner.Create()
                             .UsePort(5001)
                             .Receives<string>()
                             .Returns<string>()
                             .UsesFunction<Function>((function, input, context) => function.FunctionHandler(input, context))
                             .Build()
                             .RunAsync();

        }
    }
}
