using MassTransit;
using Stargate.MessageProcessor.Messages;
using Stargate.Persistence.Sql;

namespace Stargate.MessageProcessor.Handlers;

public class MigrateDatabase(StargateDbContext dbContext) : IConsumer<StargateMessageProcessorStarted>
{
    public async Task Consume(ConsumeContext<StargateMessageProcessorStarted> context)
    {
        if (!await dbContext.Database.CanConnectAsync(context.CancellationToken))
        {
            await dbContext.Database.EnsureCreatedAsync(context.CancellationToken);
        }
    }
}
