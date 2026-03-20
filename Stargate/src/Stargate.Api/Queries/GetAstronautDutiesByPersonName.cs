using Ardalis.Result;
using Common.LanguageExtensions.Utilities.ResultExtensions;
using MediatR;
using Stargate.Api.Dtos;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;

namespace Stargate.Api.Queries;

public class PersonWithDuties
{
    public PersonWithDuties(PersonAstronaut person, IReadOnlyCollection<AstronautDuty> duties)
    {
        Person = person;
        Duties = [.. duties];
    }

    public PersonAstronaut Person { get; private set; }

    public IReadOnlyList<AstronautDuty> Duties { get; private set; }
}

public class GetAstronautDutiesByPersonNameQuery : IRequest<Result<PersonWithDuties>>
{
    public string Name { get; set; } = string.Empty;
}

public class GetAstronautDutiesByPersonNameQueryHandler : IRequestHandler<GetAstronautDutiesByPersonNameQuery, Result<PersonWithDuties>>
{
    private readonly IPersonRepository repository;

    public GetAstronautDutiesByPersonNameQueryHandler(IPersonRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<PersonWithDuties>> Handle(GetAstronautDutiesByPersonNameQuery request, CancellationToken cancellationToken)
    {
        return repository.GetPersonByNameAsync(request.Name, cancellationToken)
            .Map(person =>
            {
                var personDto = new PersonAstronaut(person);
                return new PersonWithDuties(personDto, person.AstronautDuties);
            });
    }
}
