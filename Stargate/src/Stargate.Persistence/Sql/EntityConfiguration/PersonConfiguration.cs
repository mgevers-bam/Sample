using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Stargate.Core.Domain;

namespace Stargate.Persistence.Sql.EntityConfiguration;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();
        builder
            .HasIndex(x => x.Name)
            .IsUnique();
        builder
            .HasOne(z => z.AstronautDetail)
            .WithOne(z => z.Person)
            .HasForeignKey<AstronautDetail>(z => z.PersonId);
        builder
            .HasMany(z => z.AstronautDuties)
            .WithOne(z => z.Person)
            .HasForeignKey(z => z.PersonId);
    }
}
