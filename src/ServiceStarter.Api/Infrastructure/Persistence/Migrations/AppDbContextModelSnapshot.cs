using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ServiceStarter.Api.Infrastructure.Persistence;
using ServiceStarter.Api.Infrastructure.Persistence.Entities;

#nullable disable

namespace ServiceStarter.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
public partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("uniqueidentifier");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("datetime2");

            b.Property<string>("DisplayName")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnType("nvarchar(256)");

            b.Property<string>("Role")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("nvarchar(50)");

            b.HasKey("Id");

            b.HasIndex("Email")
                .IsUnique();

            b.ToTable("Users");
        });
#pragma warning restore 612, 618
    }
}