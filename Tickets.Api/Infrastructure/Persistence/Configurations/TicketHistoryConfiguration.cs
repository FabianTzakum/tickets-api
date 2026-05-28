using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickets.Api.Domain.Entities;

namespace Tickets.Api.Infrastructure.Persistence.Configurations;

public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
{
    public void Configure(EntityTypeBuilder<TicketHistory> builder)
    {
        builder.ToTable("TicketHistory");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.FieldName)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(history => history.OldValue)
            .HasMaxLength(250);

        builder.Property(history => history.NewValue)
            .HasMaxLength(250);

        builder.Property(history => history.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(history => history.CreatedAtUtc)
            .IsRequired();

        builder.Property(history => history.IsActive)
            .IsRequired();

        builder.HasOne(history => history.Ticket)
            .WithMany(ticket => ticket.History)
            .HasForeignKey(history => history.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(history => history.ChangedByUser)
            .WithMany()
            .HasForeignKey(history => history.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
