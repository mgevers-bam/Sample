using Microsoft.EntityFrameworkCore;
using Stargate.Core.Domain;

namespace Stargate.Persistence.Sql;

public class StargateDbContext : DbContext
{
    public StargateDbContext(DbContextOptions<StargateDbContext> options)
        : base(options)
    {
    }

    public DbSet<Person> People => Set<Person>();
    public DbSet<AstronautDetail> AstronautDetails => Set<AstronautDetail>();
    public DbSet<AstronautDuty> AstronautDuties => Set<AstronautDuty>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StargateDbContext).Assembly);
    }
}
