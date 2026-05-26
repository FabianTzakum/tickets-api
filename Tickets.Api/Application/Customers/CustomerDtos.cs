namespace Tickets.Api.Application.Customers;

public record CustomerSummaryDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? CompanyName,
    bool IsActive,
    DateTime CreatedAtUtc
);

public record CustomerDetailDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? CompanyName,
    string? Notes,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public record CreateCustomerRequest(
    string Name,
    string Email,
    string? Phone,
    string? CompanyName,
    string? Notes
);

public record UpdateCustomerRequest(
    string Name,
    string Email,
    string? Phone,
    string? CompanyName,
    string? Notes,
    bool IsActive
);
