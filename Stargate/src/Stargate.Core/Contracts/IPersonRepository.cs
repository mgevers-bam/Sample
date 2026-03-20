using Ardalis.Result;
using Common.LanguageExtensions.Contracts;
using Stargate.Core.Domain;

namespace Stargate.Core.Contracts;

public interface IPersonRepository : IUnitOfWorkRepository
{
    Task<Result<IReadOnlyList<Person>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<Person>> GetPersonByNameAsync(string name, CancellationToken cancellationToken = default);
}
