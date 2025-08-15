using Ardalis.Result;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;
using Stargate.Persistence.Sql;

namespace Stargate.Persistence.Repositories;

public class PersonRepository : EFRepository<Person>, IPersonRepository
{
    public PersonRepository(StargateDbContext context)
        : base(context)
    {
    }

    public async Task<Result<IReadOnlyList<Person>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IReadOnlyList<Person> people = await dbContext.People
                .Include(person => person.AstronautDuties)
                .Include(person => person.AstronautDetail)
                .ToListAsync(cancellationToken);

            return Result.Success(people);
        }
        catch (SqlException ex)
        {
            return Result<IReadOnlyList<Person>>.CriticalError(ex.Message);
        }
    }

    public async Task<Result<Person>> GetPersonByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var person = await dbContext.People
                .Where(person => person.Name == name)
                .Include(person => person.AstronautDuties)
                .Include(person => person.AstronautDetail)
                .SingleOrDefaultAsync(cancellationToken);

            return person == null
                ? Result<Person>.NotFound()
                : Result.Success(person);
        }
        catch (SqlException ex)
        {
            return Result<Person>.CriticalError(ex.Message);
        }
    }
}
