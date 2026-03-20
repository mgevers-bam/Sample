using Common.LanguageExtensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.ServerSentEvents;

namespace Stargate.Infrastructure.ServerSentEvents;

public static class EventPublisherServiceCollectionExtensions
{
    public static IServiceCollection AddEventPublisher(this IServiceCollection services)
    {
        return services.AddSingletonAsAllImplementedInterfaces<ServerEventPublisher>();
    }

    public static IEndpointConventionBuilder UseServerSentEvents(this IEndpointRouteBuilder app, string route)
    {
        return app.MapGet(route, (
            IServerEventPublisher eventPublisher,
            CancellationToken cancellationToken) =>
        {
            var connectionId = eventPublisher.Connect();

            var eventStream = eventPublisher.GetEventStream(connectionId);
            var typedEvents = eventStream.ReadAllAsync(cancellationToken)
                .Select(e =>
                {
                    var json = JsonConvert.SerializeObject(e, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    return new SseItem<string>(json);
                });

            return TypedResults.ServerSentEvents(typedEvents);
        });
    }
}
