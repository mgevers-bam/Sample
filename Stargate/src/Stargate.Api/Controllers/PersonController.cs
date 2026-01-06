using Ardalis.Result.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Stargate.Api.Queries;
using Stargate.Core.Commands;

namespace Stargate.Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class PersonController : ControllerBase
{
    private readonly IMediator mediator;

    public PersonController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPeople(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPeopleQuery(), cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetPersonByName(string name, CancellationToken cancellationToken)
    {
        var query = new GetPersonByNameQuery
        { 
            Name = name
        };
        var result = await mediator.Send(query, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }

    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] CreatePersonCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        IConvertToActionResult actionResult = result.ToActionResult(this);
        return actionResult.Convert();
    }
}
