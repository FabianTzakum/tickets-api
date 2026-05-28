using System.Security.Claims;
using Tickets.Api.Application.Common;

namespace Tickets.Api.Application.Auth;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<ApiResponse<AuthUserDto>> GetCurrentUserAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken);
}
