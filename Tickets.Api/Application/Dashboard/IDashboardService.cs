using Tickets.Api.Application.Common;

namespace Tickets.Api.Application.Dashboard;

public interface IDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken);
}
