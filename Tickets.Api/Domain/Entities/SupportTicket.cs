using Tickets.Api.Domain.Common;
using Tickets.Api.Domain.Enums;

namespace Tickets.Api.Domain.Entities;

public class SupportTicket : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public Guid CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public AppUser? AssignedToUser { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = [];

    public ICollection<TicketHistory> History { get; set; } = [];
}
