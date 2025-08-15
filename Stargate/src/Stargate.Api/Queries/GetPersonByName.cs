using Ardalis.Result;
using MediatR;
using Stargate.Api.Dtos;
using Stargate.Core;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;

namespace Stargate.Api.Queries;

public class GetPersonByNameQuery : IRequest<Result<PersonAstronaut>>
{
    public string Name { get; set; } = string.Empty;
}

public class GetPersonByNameQueryHandler : IRequestHandler<GetPersonByNameQuery, Result<PersonAstronaut>>
{
    private readonly IPersonRepository repository;

    public GetPersonByNameQueryHandler(IPersonRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<PersonAstronaut>> Handle(GetPersonByNameQuery request, CancellationToken cancellationToken)
    {
        return repository.GetPersonByNameAsync(request.Name, cancellationToken)
            .Map<Person, PersonAstronaut>(person => new PersonAstronaut(person));
    }
}
