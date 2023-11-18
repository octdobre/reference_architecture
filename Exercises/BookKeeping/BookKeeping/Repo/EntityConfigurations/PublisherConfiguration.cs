﻿using BookKeeping.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookKeeping.Repo.EntityConfigurations;

public class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
{
    public void Configure(EntityTypeBuilder<Publisher> modelBuilder)
    {
        //Primary Key
        modelBuilder.
            HasKey(a => a.Identity);

        modelBuilder
            .Property(a => a.Identity)
            //Identity value is generated by the database if not specified
            //If specified then no database value is generated
            .ValueGeneratedOnAdd();

        modelBuilder
            .Property(a => a.Name)
            .HasMaxLength(255);

        ////Defining one to one relationship in entity configuration
        //modelBuilder
        //    .HasOne(a => a.Editor)
        //    .WithOne(b => b.Publisher)
        //    .HasForeignKey<Editor>(b => b.PublisherId);

        modelBuilder.HasData(
            new Publisher { Identity = Guid.NewGuid(), Name = "Publisher Works" },
            new Publisher { Identity = Guid.NewGuid(), Name = "Math works"});
    }
}
