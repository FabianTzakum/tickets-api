using Tickets.Api.Domain.Enums;

namespace Tickets.Api.Application.Auth;

public record LoginRequest(
    string Email,
    string Password
);

public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    AuthUserDto User
);

public record AuthUserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role
);
