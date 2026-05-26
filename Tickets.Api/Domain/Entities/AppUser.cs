using Tickets.Api.Domain.Common;
using Tickets.Api.Domain.Enums;

namespace Tickets.Api.Domain.Entities;

public class AppUser : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Client;

    public ICollection<SupportTicket> AssignedTickets { get; set; } = [];

    public ICollection<TicketComment> Comments { get; set; } = [];
}
