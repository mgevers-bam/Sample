using MassTransit;
using Microsoft.Extensions.Hosting;
using Stargate.MessageProcessor.Messages;

namespace Stargate.MessageProcessor;

public class ServiceStartup(
    IBus bus,
    IBusControl busControl)
    : IHostedLifecycleService
{
    public Task StartingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        await busControl.WaitForHealthStatus(BusHealthStatus.Healthy, cancellationToken);
        await bus.Publish(new StargateMessageProcessorStarted(), cancellationToken);
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
