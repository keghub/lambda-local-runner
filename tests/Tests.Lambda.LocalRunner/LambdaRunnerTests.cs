using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using EMG.Lambda.LocalRunner;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Moq;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class LambdaRunnerTests
    {
        [Test, AutoData]
        public void Constructor_is_correctly_guarded_from_nulls(GuardClauseAssertion clause)
        {
            clause.Verify(typeof(LambdaRunner).GetConstructors());
        }

        [Test, AutoMoqData]
        [Ignore("Not stopping")]
        public async Task IWebHost_can_be_programmatically_stopped(
            IWebHost host,
            IHostingEnvironment hostingEnvironment,
            IServerAddressesFeature serverAddressesFeature,
            IApplicationLifetime applicationLifetime
        )
        {
            Mock.Get(host.Services).Setup(s => s.GetService(typeof(IHostingEnvironment))).Returns(hostingEnvironment);
            Mock.Get(host.Services).Setup(s => s.GetService(typeof(IApplicationLifetime))).Returns(applicationLifetime);
            Mock.Get(host.ServerFeatures).Setup(p => p.Get<IServerAddressesFeature>()).Returns(serverAddressesFeature);

            var cts = new CancellationTokenSource();

            cts.CancelAfter(TimeSpan.FromSeconds(1));

            await host.RunAsync(cts.Token);

            Mock.Get(host).Verify(p => p.StartAsync(It.IsAny<CancellationToken>()));
        }

        [Test, AutoMoqData]
        [Ignore("Not stopping")]
        public async Task RunAsync_uses_StartAsync(
            IWebHost host,
            IHostingEnvironment hostingEnvironment,
            IServerAddressesFeature serverAddressesFeature,
            IApplicationLifetime applicationLifetime
        )
        {
            var sut = new LambdaRunner(host);
            Mock.Get(host.Services).Setup(s => s.GetService(typeof(IHostingEnvironment))).Returns(hostingEnvironment);
            Mock.Get(host.Services).Setup(s => s.GetService(typeof(IApplicationLifetime))).Returns(applicationLifetime);
            Mock.Get(host.ServerFeatures).Setup(p => p.Get<IServerAddressesFeature>()).Returns(serverAddressesFeature);

            var cts = new CancellationTokenSource();

            cts.CancelAfter(TimeSpan.FromSeconds(1));

            await sut.RunAsync(cts.Token);

            Mock.Get(host).Verify(p => p.StartAsync(It.IsAny<CancellationToken>()));
        }

        [Test]
        public void Create_returns_a_builder()
        {
            Assert.That(LambdaRunner.Create(), Is.InstanceOf<IRunnerBuilder>());
        }
    }
}