using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickets.Api.Domain.Entities;

namespace Tickets.Api.Infrastructure.Persistence.Configurations;

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("TicketComments");

        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(comment => comment.IsInternal)
            .IsRequired();

        builder.Property(comment => comment.CreatedAtUtc)
            .IsRequired();

        builder.Property(comment => comment.IsActive)
            .IsRequired();

        builder.HasOne(comment => comment.AuthorUser)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
