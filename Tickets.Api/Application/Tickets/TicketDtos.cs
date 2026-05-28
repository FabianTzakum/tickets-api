using Tickets.Api.Domain.Enums;

namespace Tickets.Api.Application.Tickets;

public record TicketQueryParameters(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    TicketStatus? Status = null,
    TicketPriority? Priority = null,
    Guid? CustomerId = null,
    Guid? AssignedToUserId = null,
    string? SortBy = "createdAt",
    string? SortDirection = "desc"
);

public record TicketSummaryDto(
    Guid Id,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    string CustomerName,
    string? AssignedAgentName,
    DateTime CreatedAtUtc
);

public record TicketDetailDto(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid CustomerId,
    string CustomerName,
    Guid? AssignedToUserId,
    string? AssignedAgentName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc,
    IReadOnlyCollection<TicketCommentDto> Comments,
    IReadOnlyCollection<TicketHistoryDto> History
);

public record TicketCommentDto(
    Guid Id,
    Guid AuthorUserId,
    string AuthorName,
    string Message,
    bool IsInternal,
    DateTime CreatedAtUtc
);

public record TicketHistoryDto(
    Guid Id,
    string FieldName,
    string? OldValue,
    string? NewValue,
    string? ChangedByName,
    string Description,
    DateTime CreatedAtUtc
);

public record CreateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    Guid CustomerId,
    Guid? AssignedToUserId
);

public record UpdateTicketRequest(
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid? AssignedToUserId,
    bool IsActive
);

public record AddTicketCommentRequest(
    Guid AuthorUserId,
    string Message,
    bool IsInternal
);

public record ChangeTicketStatusRequest(
    TicketStatus Status
);
