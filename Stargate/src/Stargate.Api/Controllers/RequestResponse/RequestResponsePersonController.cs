using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Stargate.Core.Commands;

namespace Stargate.Api.Controllers.RequestResponse;

[ApiController]
[Route("[controller]")]
public class RequestResponsePersonController(IRequestClient<CreatePersonCommand> createPersonClient) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePerson([FromBody] CreatePersonCommand command, CancellationToken cancellationToken)
    {
        var result = await createPersonClient.GetResponse<Result<int>>(command, cancellationToken);

        IConvertToActionResult actionResult = result.Message.ToActionResult(this);
        return actionResult.Convert();
    }
}
