using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using EMG.Lambda.LocalRunner;
using EMG.Lambda.LocalRunner.Internal;
using NUnit.Framework;

namespace Tests.Internal
{
    [TestFixture]
    public class RunnerBuilderTests
    {
        [Test, AutoMoqData]
        public void Receives_T_returns_initialized_builder_step(InnerRunnerBuilder sut, int port, Func<ILambdaSerializer> serializerFactory, Func<ILambdaContext> contextFactory)
        {
            sut.UsePort(port);
            sut.UseSerializer(serializerFactory);
            sut.UseLambdaContext(contextFactory);

            var result = sut.Receives<string>() as InnerReceivingRunnerBuilder<string>;
            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Port, Is.EqualTo(sut.Port));
            Assert.That(result.SerializerFactory, Is.EqualTo(sut.SerializerFactory));
            Assert.That(result.LambdaContextFactory, Is.EqualTo(sut.LambdaContextFactory));
        }

        [Test, AutoMoqData]
        public void Returns_T_returns_initialized_builder_step(InnerReceivingRunnerBuilder<string> sut)
        {
            var result = sut.Returns<string>() as InnerReturningRunnerBuilder<string, string>;

            Assert.That(result, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void UsesAsyncFunctionWithNoResult_returns_initialized_builder_step(InnerReceivingRunnerBuilder<string> sut, Func<object, string, ILambdaContext, Task> executor)
        {
            var result = sut.UsesAsyncFunctionWithNoResult(executor) as InnerFunctionRunnerBuilder<object>;

            Assert.That(result, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void UsesFunctionWithNoResult_returns_initialized_builder_step(InnerReceivingRunnerBuilder<string> sut, Action<object, string, ILambdaContext> executor)
        {
            var result = sut.UsesFunctionWithNoResult(executor) as InnerFunctionRunnerBuilder<object>;

            Assert.That(result, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void UsesAsyncFunction_returns_initialized_builder_step(InnerReturningRunnerBuilder<string, string> sut, Func<object, string, ILambdaContext, Task<string>> executor)
        {
            var result = sut.UsesAsyncFunction(executor) as InnerFunctionRunnerBuilder<object>;

            Assert.That(result, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void UsesFunction_returns_initialized_builder_step(InnerReturningRunnerBuilder<string, string> sut, Func<object, string, ILambdaContext, string> executor)
        {
            var result = sut.UsesFunction(executor) as InnerFunctionRunnerBuilder<object>;

            Assert.That(result, Is.Not.Null);
        }

        [Test, AutoMoqData]
        public void WithResponseContentType_initializes_content_type(InnerReturningRunnerBuilder<string, string> sut, Func<object, string, ILambdaContext, string> executor, string contentType)
        {
            var result = sut.WithResponseContentType(contentType);

            Assert.That(sut.ResponseContentType, Is.EqualTo(contentType));
        }

        [Test, AutoMoqData]
        public void Build_returns_a_initialized_runner(InnerFunctionRunnerBuilder<object> sut)
        {
            var result = sut.Build() as LambdaRunner;

            Assert.That(result, Is.Not.Null);
        }
    }
}
