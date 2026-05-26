using Microsoft.EntityFrameworkCore;
using Tickets.Api.Application.Common;
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

        var totalTickets = await dbContext.Tickets
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var openTickets = await CountTicketsByStatusAsync(TicketStatus.Open, cancellationToken);
        var inProgressTickets = await CountTicketsByStatusAsync(TicketStatus.InProgress, cancellationToken);
        var waitingClientTickets = await CountTicketsByStatusAsync(TicketStatus.WaitingClient, cancellationToken);
        var resolvedTickets = await CountTicketsByStatusAsync(TicketStatus.Resolved, cancellationToken);
        var closedTickets = await CountTicketsByStatusAsync(TicketStatus.Closed, cancellationToken);

        var criticalTickets = await CountTicketsByPriorityAsync(TicketPriority.Critical, cancellationToken);
        var highPriorityTickets = await CountTicketsByPriorityAsync(TicketPriority.High, cancellationToken);

        var unassignedTickets = await dbContext.Tickets
            .AsNoTracking()
            .CountAsync(ticket => ticket.AssignedToUserId == null, cancellationToken);

        var ticketsByStatus = await dbContext.Tickets
            .AsNoTracking()
            .GroupBy(ticket => ticket.Status)
            .Select(group => new TicketStatusMetricDto(
                group.Key.ToString(),
                group.Count()
            ))
            .ToListAsync(cancellationToken);

        var ticketsByPriority = await dbContext.Tickets
            .AsNoTracking()
            .GroupBy(ticket => ticket.Priority)
            .Select(group => new TicketPriorityMetricDto(
                group.Key.ToString(),
                group.Count()
            ))
            .ToListAsync(cancellationToken);

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
            ticketsByStatus,
            ticketsByPriority
        );

        return ApiResponse<DashboardSummaryDto>.Ok(summary, "Resumen del dashboard obtenido correctamente.");
    }

    private Task<int> CountTicketsByStatusAsync(TicketStatus status, CancellationToken cancellationToken)
    {
        return dbContext.Tickets
            .AsNoTracking()
            .CountAsync(ticket => ticket.Status == status, cancellationToken);
    }

    private Task<int> CountTicketsByPriorityAsync(TicketPriority priority, CancellationToken cancellationToken)
    {
        return dbContext.Tickets
            .AsNoTracking()
            .CountAsync(ticket => ticket.Priority == priority, cancellationToken);
    }
}
