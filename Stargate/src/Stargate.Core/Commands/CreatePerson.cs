using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;

namespace Stargate.Core.Commands;

public class CreatePersonCommand : IRequest<Result<int>>
{
    public required string Name { get; set; } = string.Empty;
}

public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, Result<int>>
{
    private readonly ILogger<CreatePersonCommandHandler> _logger;
    private readonly IRepository<Person> _repository;

    public CreatePersonCommandHandler(
        ILogger<CreatePersonCommandHandler> logger,
        IRepository<Person> repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<Result<int>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = new Person(request.Name);

        _repository.Add(person);
        var commitResult = await _repository.CommitTransaction(cancellationToken);

        if (!commitResult.IsSuccess)
        {
            _logger.LogError(
                "Failed to commit transaction for creating person {Name}: {Error}",
                request.Name,
                string.Join(",", commitResult.Errors));

            return commitResult.AsTypedError<int>();
        }

        _logger.LogInformation("Created person {Name} with ID {Id}", person.Name, person.Id);
        return person.Id;
    }
}
