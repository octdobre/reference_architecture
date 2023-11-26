using BookKeeping.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping.Repo.EntityConfigurations;

public class ReaderConfiguration : IEntityTypeConfiguration<Reader>
{
    public void Configure(EntityTypeBuilder<Reader> modelBuilder)
    {
        //Primary Key
        modelBuilder.
            HasKey(r => r.Identity);

        modelBuilder
            .Property(r => r.Identity)
            .ValueGeneratedOnAdd();

        //Configuring a column as a ConcurrencyToken
        modelBuilder
            .Property(r => r.LastName)
            .IsConcurrencyToken();
    }
}