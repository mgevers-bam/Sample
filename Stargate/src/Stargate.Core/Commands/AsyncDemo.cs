using Common.LanguageExtensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Stargate.Core.Commands;

public record BadAsync : IRequest { public int RequestCount { get; set; } = 10; }

public record BetterAsync : IRequest { public int RequestCount { get; set; } = 10; }

public record BestAsync : IRequest { public int RequestCount { get; set; } = 10; }

public class AsyncDemo(HttpClient httpClient, ILogger<AsyncDemo> logger) :
    IRequestHandler<BadAsync>,
    IRequestHandler<BetterAsync>,
    IRequestHandler<BestAsync>
{
    public Task Handle(BadAsync request, CancellationToken cancellationToken)
    {
        var enumerable = Enumerable.Range(0, request.RequestCount);

        foreach (var i in enumerable)
        {
            var response = PingApi(cancellationToken).Result;
            logger.LogDebug("Ping Api responded with {Response}", response);
        }

        logger.LogInformation("Finished processing BadAsync request with {RequestCount} requests", request.RequestCount);
        return Task.CompletedTask;
    }

    public async Task Handle(BetterAsync request, CancellationToken cancellationToken)
    {
        var enumerable = Enumerable.Range(0, request.RequestCount);

        foreach (var i in enumerable)
        {
            var response = await PingApi(cancellationToken);
            logger.LogDebug("Ping Api responded with {Response}", response);
        }

        logger.LogInformation("Finished processing BetterAsync request with {RequestCount} requests", request.RequestCount);
    }

    public async Task Handle(BestAsync request, CancellationToken cancellationToken)
    {
        var enumerable = Enumerable.Range(0, request.RequestCount);

        var tasks = enumerable
            .Select(async i => 
            {
                var response = await PingApi(cancellationToken);
                logger.LogDebug("Ping Api responded with {Response}", response);
            })
            .ToList();

        await Task.WhenAll(tasks);

        logger.LogInformation("Finished processing BestAsync request with {RequestCount} requests", request.RequestCount);
    }

    private async Task<string> PingApi(CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync("http://host.docker.internal:5003/Ping", cancellationToken: cancellationToken);

        response.EnsureSuccessStatusCode();

        return "Pong";
    }
}
