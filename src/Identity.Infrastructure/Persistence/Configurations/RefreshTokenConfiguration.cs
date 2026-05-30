using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Id)
                .ValueGeneratedNever();

            builder.Property(rt => rt.CreatedAt)
                .ValueGeneratedNever();

            builder.Property(rt => rt.UpdatedAt)
                .ValueGeneratedNever();

            builder.Property(rt => rt.UserId)
                .IsRequired();

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(rt => rt.ExpiresAt)
                .IsRequired();

            builder.Property(rt => rt.RevokedAt)
                .IsRequired(false);

            builder.HasIndex(rt => rt.UserId);

            builder.HasIndex(rt => rt.Token)
                .IsUnique();
        }
    }
}
