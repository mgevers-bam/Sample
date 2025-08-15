using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Stargate.Api.Queries;
using Stargate.Core.Commands;

namespace Stargate.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AstronautDutyController : ControllerBase
{
    private readonly IMediator mediator;

    public AstronautDutyController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetAstronautDutiesByName(string name, CancellationToken cancellationToken)
    {
        var query = new GetAstronautDutiesByPersonNameQuery
        {
            Name = name
        };
        var result = await mediator.Send(query, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDutyCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }
}
