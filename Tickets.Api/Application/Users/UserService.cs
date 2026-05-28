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

        return ApiResponse<IReadOnlyCollection<UserSummaryDto>>.Ok(
            users,
            "Usuarios obtenidos correctamente."
        );
    }

    public async Task<ApiResponse<UserSummaryDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (user is null)
        {
            return ApiResponse<UserSummaryDto>.Fail("No se encontró el usuario solicitado.");
        }

        return ApiResponse<UserSummaryDto>.Ok(
            MapToSummaryDto(user),
            "Usuario obtenido correctamente."
        );
    }

    public async Task<ApiResponse<UserSummaryDto>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateCreate(request);

        if (errors.Count > 0)
        {
            return ApiResponse<UserSummaryDto>.Fail("La validación del usuario falló.", errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Users
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<UserSummaryDto>.Fail(
                "El correo del usuario ya existe.",
                ["El correo debe ser único."]
            );
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

        return ApiResponse<UserSummaryDto>.Ok(
            MapToSummaryDto(user),
            "Usuario creado correctamente."
        );
    }

    public async Task<ApiResponse<UserSummaryDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateUpdate(request);

        if (errors.Count > 0)
        {
            return ApiResponse<UserSummaryDto>.Fail("La validación del usuario falló.", errors);
        }

        var user = await dbContext.Users
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (user is null)
        {
            return ApiResponse<UserSummaryDto>.Fail("No se encontró el usuario solicitado.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Users
            .AnyAsync(item => item.Id != id && item.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<UserSummaryDto>.Fail(
                "El correo del usuario ya existe.",
                ["El correo debe ser único."]
            );
        }

        user.FullName = request.FullName.Trim();
        user.Email = normalizedEmail;
        user.Role = request.Role;
        user.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<UserSummaryDto>.Ok(
            MapToSummaryDto(user),
            "Usuario actualizado correctamente."
        );
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
            errors.Add("La contraseña es obligatoria.");
        }

        if (request.Password.Length < 8)
        {
            errors.Add("La contraseña debe contener al menos 8 caracteres.");
        }

        return errors;
    }

    private static List<string> ValidateUpdate(UpdateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            errors.Add("El nombre completo es obligatorio.");
        }

        if (request.FullName.Length > 160)
        {
            errors.Add("El nombre completo no puede exceder 160 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("El correo es obligatorio.");
        }

        if (!request.Email.Contains('@', StringComparison.Ordinal) || request.Email.Length > 180)
        {
            errors.Add("El correo no tiene un formato válido.");
        }

        return errors;
    }
}
