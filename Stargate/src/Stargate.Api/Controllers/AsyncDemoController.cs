using MediatR;
using Microsoft.AspNetCore.Mvc;
using Stargate.Core.Commands;

namespace Stargate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AsyncDemoController(IMediator mediator) : ControllerBase
{
    [HttpPost("Bad")]
    public async Task<IActionResult> BadAsync(CancellationToken cancellationToken)
    {
        await mediator.Send(new BadAsync() { RequestCount = 10 }, cancellationToken);

        return Ok();
    }

    [HttpPost("Better")]
    public async Task<IActionResult> BetterAsync(CancellationToken cancellationToken)
    {
        await mediator.Send(new BetterAsync() { RequestCount = 10 }, cancellationToken);

        return Ok();
    }

    [HttpPost("Best")]
    public async Task<IActionResult> BestAsync(CancellationToken cancellationToken)
    {
        await mediator.Send(new BestAsync() { RequestCount = 10 }, cancellationToken);

        return Ok();
    }
}
