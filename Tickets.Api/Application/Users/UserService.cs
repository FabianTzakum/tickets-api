using Microsoft.EntityFrameworkCore;
using Tickets.Api.Application.Common;
using Tickets.Api.Domain.Entities;
using Tickets.Api.Infrastructure.Persistence;

namespace Tickets.Api.Application.Users;

public class UserService(TicketsDbContext dbContext) : IUserService
{
    public async Task<ApiResponse<IReadOnlyCollection<UserSummaryDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .AsNoTracking()
            .OrderBy(user => user.FullName)
            .Select(user => new UserSummaryDto(
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.IsActive,
                user.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<UserSummaryDto>>.Ok(users);
    }

    public async Task<ApiResponse<UserSummaryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (user is null)
        {
            return ApiResponse<UserSummaryDto>.Fail("User was not found.");
        }

        return ApiResponse<UserSummaryDto>.Ok(MapToSummaryDto(user));
    }

    public async Task<ApiResponse<UserSummaryDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateCreate(request);

        if (errors.Count > 0)
        {
            return ApiResponse<UserSummaryDto>.Fail("User validation failed.", errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Users
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<UserSummaryDto>.Fail("User email already exists.", ["Email must be unique."]);
        }

        var user = new AppUser
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role
        };

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<UserSummaryDto>.Ok(MapToSummaryDto(user), "User created successfully.");
    }

    public async Task<ApiResponse<UserSummaryDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateUpdate(request);

        if (errors.Count > 0)
        {
            return ApiResponse<UserSummaryDto>.Fail("User validation failed.", errors);
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (user is null)
        {
            return ApiResponse<UserSummaryDto>.Fail("User was not found.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Users
            .AnyAsync(item => item.Id != id && item.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<UserSummaryDto>.Fail("User email already exists.", ["Email must be unique."]);
        }

        user.FullName = request.FullName.Trim();
        user.Email = normalizedEmail;
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<UserSummaryDto>.Ok(MapToSummaryDto(user), "User updated successfully.");
    }

    private static UserSummaryDto MapToSummaryDto(AppUser user)
    {
        return new UserSummaryDto(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc
        );
    }

    private static List<string> ValidateCreate(CreateUserRequest request)
    {
        var errors = ValidateUpdate(new UpdateUserRequest(
            request.FullName,
            request.Email,
            request.Role,
            true
        ));

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }

        if (request.Password.Length < 8)
        {
            errors.Add("Password must contain at least 8 characters.");
        }

        return errors;
    }

    private static List<string> ValidateUpdate(UpdateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors.Add("Full name is required.");
        }

        if (request.FullName.Length > 160)
        {
            errors.Add("Full name cannot exceed 160 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }

        if (!request.Email.Contains('@', StringComparison.Ordinal) || request.Email.Length > 180)
        {
            errors.Add("Email is not valid.");
        }

        return errors;
    }
}
