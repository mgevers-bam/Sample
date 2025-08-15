using Ardalis.Result;
using Stargate.Core.Domain;

namespace Stargate.Core.Contracts;

public interface IPersonRepository : IRepository<Person>
{
    Task<Result<IReadOnlyList<Person>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<Person>> GetPersonByNameAsync(string name, CancellationToken cancellationToken = default);
}
