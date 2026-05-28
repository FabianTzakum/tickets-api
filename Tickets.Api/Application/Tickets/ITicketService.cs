using Tickets.Api.Application.Common;

namespace Tickets.Api.Application.Tickets;

public interface ITicketService
{
    Task<ApiResponse<PagedResponse<TicketSummaryDto>>> GetAllAsync(
        TicketQueryParameters queryParameters,
        CancellationToken cancellationToken);

    Task<ApiResponse<TicketDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ApiResponse<TicketDetailDto>> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken);

    Task<ApiResponse<TicketDetailDto>> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken);

    Task<ApiResponse<TicketDetailDto>> AddCommentAsync(Guid ticketId, AddTicketCommentRequest request, CancellationToken cancellationToken);

    Task<ApiResponse<TicketDetailDto>> ChangeStatusAsync(Guid ticketId, ChangeTicketStatusRequest request, CancellationToken cancellationToken);
}
