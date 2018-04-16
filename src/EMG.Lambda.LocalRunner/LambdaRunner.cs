using System;
using System.Threading;
using System.Threading.Tasks;
using EMG.Lambda.LocalRunner.Internal;
using Microsoft.AspNetCore.Hosting;

namespace EMG.Lambda.LocalRunner
{
    public interface IRunner
    {
        Task RunAsync(CancellationToken cancellationToken = default(CancellationToken));
    }

    public class LambdaRunner : IRunner
    {
        private readonly IWebHost _host;

        internal LambdaRunner(IWebHost host)
        {
            _host = host ?? throw new ArgumentNullException(nameof(host));
        }

        public async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken)) => await _host.RunAsync(cancellationToken);

        public static IRunnerBuilder Create() => new InnerRunnerBuilder();
    }
}