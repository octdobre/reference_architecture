﻿using BookKeeping.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping.Repo.EntityConfigurations;

public class CabinetConfiguration : IEntityTypeConfiguration<Cabinet>
{
    public void Configure(EntityTypeBuilder<Cabinet> modelBuilder)
    {
        //Primary Key
        modelBuilder.HasKey(c => c.Identity);

        modelBuilder
            .Property(c => c.Identity)
            //Identity value is generated by the database if not specified
            //If specified then no database value is generated
            .ValueGeneratedOnAdd();

        modelBuilder
            .Property(c => c.Category);

        // Configuring a private collection.
        //modelBuilder.Navigation(x => x.Shelves).Metadata.SetField("shelves");
        //modelBuilder.Navigation(x => x.Shelves).UsePropertyAccessMode(PropertyAccessMode.Field);

        modelBuilder.OwnsMany<Shelf>(
            c => c.Shelves,
            s =>
            {
                s.ToTable("Shelves");

                s.Property<int>(ss => ss.RowNumber);
            });
    }
}