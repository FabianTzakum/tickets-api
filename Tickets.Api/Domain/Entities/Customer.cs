using Tickets.Api.Domain.Common;

namespace Tickets.Api.Domain.Entities;

public class Customer : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? CompanyName { get; set; }

    public string? Notes { get; set; }

    public ICollection<SupportTicket> Tickets { get; set; } = [];
}
