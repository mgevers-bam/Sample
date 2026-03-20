using Ardalis.Result;
using Common.LanguageExtensions.Utilities.ResultExtensions;
using MediatR;
using Stargate.Api.Dtos;
using Stargate.Core.Contracts;

namespace Stargate.Api.Queries;

public class GetPeopleQuery : IRequest<Result<IReadOnlyList<PersonAstronaut>>> { }

public class GetPeopleQueryHandler : IRequestHandler<GetPeopleQuery, Result<IReadOnlyList<PersonAstronaut>>>
{
    private readonly IPersonRepository repository;

    public GetPeopleQueryHandler(IPersonRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<IReadOnlyList<PersonAstronaut>>> Handle(GetPeopleQuery request, CancellationToken cancellationToken)
    {
        return repository.GetAllAsync(cancellationToken)
            .Map(people =>
            {
                IReadOnlyList<PersonAstronaut> astronauts = people
                    .Select(person => new PersonAstronaut(person))
                    .ToList();

                return astronauts;
            });
    }
}
