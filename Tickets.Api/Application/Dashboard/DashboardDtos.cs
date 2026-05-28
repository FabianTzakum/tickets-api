namespace Tickets.Api.Application.Dashboard;

public record DashboardSummaryDto(
    int TotalCustomers,
    int TotalUsers,
    int TotalTickets,
    int OpenTickets,
    int InProgressTickets,
    int WaitingClientTickets,
    int ResolvedTickets,
    int ClosedTickets,
    int CriticalTickets,
    int HighPriorityTickets,
    int UnassignedTickets,
    int OverdueTickets,
    IReadOnlyCollection<TicketStatusMetricDto> TicketsByStatus,
    IReadOnlyCollection<TicketPriorityMetricDto> TicketsByPriority
);

public record TicketStatusMetricDto(
    string Status,
    int Count
);

public record TicketPriorityMetricDto(
    string Priority,
    int Count
);
