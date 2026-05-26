using Tickets.Api.Domain.Enums;

namespace Tickets.Api.Application.Users;

public record UserSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    DateTime CreatedAtUtc
);

public record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role
);

public record UpdateUserRequest(
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive
);
