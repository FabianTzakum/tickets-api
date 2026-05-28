using Tickets.Api.Domain.Enums;

namespace Tickets.Api.Application.Tickets;

public static class TicketStateTransitionValidator
{
    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        [TicketStatus.Open] =
        [
            TicketStatus.InProgress
        ],
        [TicketStatus.InProgress] =
        [
            TicketStatus.WaitingClient,
            TicketStatus.Resolved
        ],
        [TicketStatus.WaitingClient] =
        [
            TicketStatus.InProgress,
            TicketStatus.Resolved
        ],
        [TicketStatus.Resolved] =
        [
            TicketStatus.Closed,
            TicketStatus.InProgress
        ],
        [TicketStatus.Closed] =
        [
        ]
    };

    public static bool CanTransition(TicketStatus currentStatus, TicketStatus nextStatus)
    {
        if (currentStatus == nextStatus)
        {
            return true;
        }

        return AllowedTransitions.TryGetValue(currentStatus, out var allowedStatuses)
            && allowedStatuses.Contains(nextStatus);
    }

    public static string GetInvalidTransitionMessage(TicketStatus currentStatus, TicketStatus nextStatus)
    {
        return $"No se puede cambiar el ticket de '{GetDisplayName(currentStatus)}' a '{GetDisplayName(nextStatus)}'.";
    }

    public static string GetDisplayName(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.Open => "Abierto",
            TicketStatus.InProgress => "En progreso",
            TicketStatus.WaitingClient => "En espera del cliente",
            TicketStatus.Resolved => "Resuelto",
            TicketStatus.Closed => "Cerrado",
            _ => status.ToString()
        };
    }
}
