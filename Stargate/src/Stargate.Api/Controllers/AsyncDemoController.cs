using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stargate.Core.Commands;

namespace Stargate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AsyncDemoController(
    IMediator mediator,
    ILogger<AsyncDemoController> logger)
    : ControllerBase
{
    [HttpPost("Bad")]
    public async Task<IActionResult> BadAsync(AsyncDemoRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new BadAsync() { RequestCount = request.RequestCount }, cancellationToken);

        return Ok();
    }

    [HttpPost("Better")]
    public async Task<IActionResult> BetterAsync(AsyncDemoRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new BetterAsync() { RequestCount = request.RequestCount }, cancellationToken);

        return Ok();
    }

    [Authorize]
    [HttpPost("Best")]
    public async Task<IActionResult> BestAsync(AsyncDemoRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new BestAsync() { RequestCount = request.RequestCount }, cancellationToken);

        return Ok();
    }
}

public class AsyncDemoRequest
{
    public int RequestCount { get; set; } = 10;
}

public class ClaimViewModel
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
