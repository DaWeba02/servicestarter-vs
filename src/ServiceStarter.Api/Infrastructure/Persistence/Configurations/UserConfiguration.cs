using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceStarter.Api.Infrastructure.Persistence.Entities;

namespace ServiceStarter.Api.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();
    }
}
