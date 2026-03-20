using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Stargate.Core.Commands;

namespace Stargate.Api.Controllers.Async;

[ApiController]
[Route("[controller]")]
public class AsyncPersonController(IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] CreatePersonCommand command, CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(command, cancellationToken);

        return Accepted();
    }
}
