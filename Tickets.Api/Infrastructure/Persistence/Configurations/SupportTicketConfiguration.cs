using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickets.Api.Domain.Entities;

namespace Tickets.Api.Infrastructure.Persistence.Configurations;

public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Title)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(ticket => ticket.Description)
            .HasMaxLength(3000)
            .IsRequired();

        builder.Property(ticket => ticket.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(ticket => ticket.Priority)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(ticket => ticket.CreatedAtUtc)
            .IsRequired();

        builder.Property(ticket => ticket.IsActive)
            .IsRequired();

        builder.HasIndex(ticket => ticket.Status);

        builder.HasIndex(ticket => ticket.Priority);

        builder.HasOne(ticket => ticket.AssignedToUser)
            .WithMany(user => user.AssignedTickets)
            .HasForeignKey(ticket => ticket.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(ticket => ticket.Comments)
            .WithOne(comment => comment.Ticket)
            .HasForeignKey(comment => comment.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
