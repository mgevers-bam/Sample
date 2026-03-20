using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Stargate.Infrastructure.ServerSentEvents;

public class ServerEvent
{
    public ServerEvent(object @event)
    {
        Type = @event.GetType().Name;
        Data = JsonConvert.SerializeObject(@event, new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }

    protected ServerEvent()
    {
    }

    public string Type { get; set; } = null!;
    public string Data { get; set; } = null!;
}
