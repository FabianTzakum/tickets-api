using Tickets.Api.Domain.Common;

namespace Tickets.Api.Domain.Entities;

public class TicketComment : AuditableEntity
{
    public Guid TicketId { get; set; }

    public SupportTicket? Ticket { get; set; }

    public Guid AuthorUserId { get; set; }

    public AppUser? AuthorUser { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsInternal { get; set; }
}
