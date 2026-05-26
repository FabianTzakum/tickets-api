using Tickets.Api.Application.Common;

namespace Tickets.Api.Application.Users;

public interface IUserService
{
    Task<ApiResponse<IReadOnlyCollection<UserSummaryDto>>> GetAllAsync(CancellationToken cancellationToken);

    Task<ApiResponse<UserSummaryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ApiResponse<UserSummaryDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken);

    Task<ApiResponse<UserSummaryDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken);
}
