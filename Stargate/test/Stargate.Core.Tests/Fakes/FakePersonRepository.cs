using Ardalis.Result;
using Common.Testing.Persistence;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;

namespace Stargate.Core.Tests.Fakes;

public class FakePersonRepository : FakeUnitOfWorkRepository, IPersonRepository
{
    public Task<Result<IReadOnlyList<Person>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return this.Query<Person, int>(person => true, cancellationToken);
    }

    public Task<Result<Person>> GetPersonByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return this.Find<Person, int>(person => person.Name == name, cancellationToken);
    }
}
