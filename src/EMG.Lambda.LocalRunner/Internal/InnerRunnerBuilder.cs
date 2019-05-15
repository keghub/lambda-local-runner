using System;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.TestUtilities;

namespace EMG.Lambda.LocalRunner.Internal
{
    public class InnerRunnerBuilder : IRunnerBuilder
    {
        public int Port { get; set; } = 5000;

        public Func<ILambdaSerializer> SerializerFactory { get; set; } = () => new JsonSerializer();
        public Func<ILambdaContext> LambdaContextFactory { get; set; } = () => new TestLambdaContext();

        public IRunnerBuilder UsePort(int port)
        {
            Port = port;
            return this;
        }

        public IRunnerBuilder UseSerializer<TSerializer>(Func<TSerializer> serializerFactory)
            where TSerializer : ILambdaSerializer
        {
            SerializerFactory = () => serializerFactory();
            return this;
        }

        public IReceivingRunnerBuilder<TInput> Receives<TInput>()
        {
            return new InnerReceivingRunnerBuilder<TInput>
            {
                Port = Port,
                SerializerFactory = SerializerFactory,
                LambdaContextFactory = LambdaContextFactory
            };
        }

        public IRunnerBuilder UseLambdaContext<TLambdaContext>(Func<TLambdaContext> lambdaContextFactory) where TLambdaContext : ILambdaContext
        {
            LambdaContextFactory = () => lambdaContextFactory();
            return this;
        }
    }
}