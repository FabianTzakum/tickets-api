using Microsoft.EntityFrameworkCore;
using Tickets.Api.Application.Common;
using Tickets.Api.Application.Tickets;
using Tickets.Api.Domain.Enums;
using Tickets.Api.Infrastructure.Persistence;

namespace Tickets.Api.Application.Dashboard;

public class DashboardService(TicketsDbContext dbContext) : IDashboardService
{
    public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var totalCustomers = await dbContext.Customers
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalUsers = await dbContext.Users
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var tickets = await dbContext.Tickets
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalTickets = tickets.Count;

        var openTickets = CountTicketsByStatus(tickets, TicketStatus.Open);
        var inProgressTickets = CountTicketsByStatus(tickets, TicketStatus.InProgress);
        var waitingClientTickets = CountTicketsByStatus(tickets, TicketStatus.WaitingClient);
        var resolvedTickets = CountTicketsByStatus(tickets, TicketStatus.Resolved);
        var closedTickets = CountTicketsByStatus(tickets, TicketStatus.Closed);

        var criticalTickets = CountTicketsByPriority(tickets, TicketPriority.Critical);
        var highPriorityTickets = CountTicketsByPriority(tickets, TicketPriority.High);

        var unassignedTickets = tickets.Count(ticket => ticket.AssignedToUserId == null);

        var overdueTickets = tickets.Count(ticket =>
            TicketSlaCalculator.IsOverdue(ticket.CreatedAtUtc, ticket.Priority, ticket.Status));

        var ticketsByStatus = tickets
            .GroupBy(ticket => ticket.Status)
            .Select(group => new TicketStatusMetricDto(
                group.Key.ToString(),
                group.Count()
            ))
            .ToList();

        var ticketsByPriority = tickets
            .GroupBy(ticket => ticket.Priority)
            .Select(group => new TicketPriorityMetricDto(
                group.Key.ToString(),
                group.Count()
            ))
            .ToList();

        var summary = new DashboardSummaryDto(
            totalCustomers,
            totalUsers,
            totalTickets,
            openTickets,
            inProgressTickets,
            waitingClientTickets,
            resolvedTickets,
            closedTickets,
            criticalTickets,
            highPriorityTickets,
            unassignedTickets,
            overdueTickets,
            ticketsByStatus,
            ticketsByPriority
        );

        return ApiResponse<DashboardSummaryDto>.Ok(summary, "Resumen del dashboard obtenido correctamente.");
    }

    private static int CountTicketsByStatus(IReadOnlyCollection<Domain.Entities.SupportTicket> tickets, TicketStatus status)
    {
        return tickets.Count(ticket => ticket.Status == status);
    }

    private static int CountTicketsByPriority(IReadOnlyCollection<Domain.Entities.SupportTicket> tickets, TicketPriority priority)
    {
        return tickets.Count(ticket => ticket.Priority == priority);
    }
}
