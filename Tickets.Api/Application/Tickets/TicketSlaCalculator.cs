using Tickets.Api.Domain.Enums;

namespace Tickets.Api.Application.Tickets;

public static class TicketSlaCalculator
{
    public static DateTime GetDueAtUtc(DateTime createdAtUtc, TicketPriority priority)
    {
        return createdAtUtc.AddHours(GetSlaHours(priority));
    }

    public static int GetSlaHours(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Critical => 4,
            TicketPriority.High => 8,
            TicketPriority.Medium => 24,
            TicketPriority.Low => 72,
            _ => 24
        };
    }

    public static bool IsOverdue(DateTime createdAtUtc, TicketPriority priority, TicketStatus status)
    {
        if (status is TicketStatus.Resolved or TicketStatus.Closed)
        {
            return false;
        }

        return DateTime.UtcNow > GetDueAtUtc(createdAtUtc, priority);
    }

    public static double RemainingHours(DateTime createdAtUtc, TicketPriority priority, TicketStatus status)
    {
        if (status is TicketStatus.Resolved or TicketStatus.Closed)
        {
            return 0;
        }

        var remaining = GetDueAtUtc(createdAtUtc, priority) - DateTime.UtcNow;

        return Math.Round(remaining.TotalHours, 2);
    }
}
