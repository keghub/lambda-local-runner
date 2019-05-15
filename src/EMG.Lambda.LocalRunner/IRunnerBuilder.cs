using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace EMG.Lambda.LocalRunner
{
    public interface IRunnerBuilder
    {
        IRunnerBuilder UsePort(int port);

        IRunnerBuilder UseSerializer<TSerializer>(Func<TSerializer> serializerFactory)
            where TSerializer : ILambdaSerializer;

        IRunnerBuilder UseLambdaContext<TLambdaContext>(Func<TLambdaContext> lambdaContextFactory)
            where TLambdaContext : ILambdaContext;

        IReceivingRunnerBuilder<TInput> Receives<TInput>();
    }

    public interface IReceivingRunnerBuilder<out TInput>
    {
        IReturningRunnerBuilder<TInput, TOutput> Returns<TOutput>();

        IFunctionRunnerBuilder<TFunction> UsesAsyncFunctionWithNoResult<TFunction>(Func<TFunction, TInput, ILambdaContext, Task> executor)
            where TFunction : class, new();

        IFunctionRunnerBuilder<TFunction> UsesFunctionWithNoResult<TFunction>(Action<TFunction, TInput, ILambdaContext> executor)
            where TFunction : class, new();
    }

    public interface IReturningRunnerBuilder<out TInput, TOutput>
    {
        IFunctionRunnerBuilder<TFunction> UsesAsyncFunction<TFunction>(Func<TFunction, TInput, ILambdaContext, Task<TOutput>> executor)
            where TFunction : class, new();

        IFunctionRunnerBuilder<TFunction> UsesFunction<TFunction>(Func<TFunction, TInput, ILambdaContext, TOutput> executor)
            where TFunction : class, new();
    }

    public interface IFunctionRunnerBuilder<TFunction>
        where TFunction : class, new()
    {
        IRunner Build();
    }
}