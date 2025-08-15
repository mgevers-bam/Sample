using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stargate.Persistence.Repositories;
using Stargate.Persistence.Sql;
using Stargate.Testing;

namespace Stargate.Persistence.Tests;

public class PersonRepositoryTests
{
    [Fact]
    public async Task CrudTest()
    {
        var personName = "James Bond";
        var serviceProvider = GetServiceProvider();
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<StargateDbContext>>();
        
        // Create database
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            await dbContext.Database.MigrateAsync();
        }

        // create person
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            var repo = new PersonRepository(dbContext);

            repo.Add(DataModels.CreatePerson(personName));
            var saveResult = await repo.CommitTransaction();
            Assert.True(saveResult.IsSuccess, string.Join(",", saveResult.Errors));
        }

        // add astronaut duty to person
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            var repo = new PersonRepository(dbContext);

            var personResult = await repo.GetPersonByNameAsync(personName);
            Assert.True(personResult.IsSuccess, string.Join(",", personResult.Errors));
            var duty = DataModels.CreateAstronautDuty(personResult.Value);
            personResult.Value.AddAstronautDuty(duty.Rank, duty.DutyTitle, duty.DutyStartDate);

            repo.Update(personResult.Value);
            var updateResult = await repo.CommitTransaction();
            Assert.True(updateResult.IsSuccess, string.Join(",", updateResult.Errors));
        }

        // remove person
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            var repo = new PersonRepository(dbContext);

            var personResult = await repo.GetPersonByNameAsync(personName);
            Assert.True(personResult.IsSuccess, string.Join(",", personResult.Errors));

            repo.Remove(personResult.Value);
            var deleteResult = await repo.CommitTransaction();
            Assert.True(deleteResult.IsSuccess, string.Join(",", deleteResult.Errors));
        }

        // assert person is removed
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            var repo = new PersonRepository(dbContext);

            var personResult = await repo.GetPersonByNameAsync(personName);
            Assert.Equal(ResultStatus.NotFound, personResult.Status);
        }

        // Delete database
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
    }

    private static IServiceProvider GetServiceProvider()
    {
        var services = new ServiceCollection();

        services
            .AddDbContextFactory<StargateDbContext>(options =>
            {
                options.UseSqlite("Data Source=starbase.db");
            })
            .AddDbContext<StargateDbContext>()
            .AddScopedAsAllImplementedInterfaces<PersonRepository>();

        return services.BuildServiceProvider();
    }
}
