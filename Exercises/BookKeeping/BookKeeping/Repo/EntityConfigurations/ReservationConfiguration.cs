using BookKeeping.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping.Repo.EntityConfigurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> modelBuilder)
    {
        //Primary Key
        modelBuilder.
            HasKey(a => a.Identity);

        modelBuilder
            .Property(a => a.Identity)
            .ValueGeneratedOnAdd();

        /*Configuring a Shadow Property
         and
         a Row Version for this type.
         Row version is used to lock the entire record when updating.
        */
        modelBuilder
            .Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}