using Tickets.Api.Domain.Common;

namespace Tickets.Api.Domain.Entities;

public class TicketHistory : AuditableEntity
{
    public Guid TicketId { get; set; }

    public SupportTicket? Ticket { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public Guid? ChangedByUserId { get; set; }

    public AppUser? ChangedByUser { get; set; }

    public string Description { get; set; } = string.Empty;
}
