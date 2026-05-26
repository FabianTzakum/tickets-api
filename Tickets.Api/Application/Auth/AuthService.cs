using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tickets.Api.Application.Common;
using Tickets.Api.Infrastructure.Persistence;

namespace Tickets.Api.Application.Auth;

public class AuthService(TicketsDbContext dbContext, IConfiguration configuration) : IAuthService
{
    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ApiResponse<LoginResponse>.Fail("El correo y la contraseña son obligatorios.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Email == normalizedEmail && item.IsActive, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponse>.Fail("Credenciales inválidas.");
        }

        var expirationMinutes = configuration.GetValue<int>("Jwt:ExpirationMinutes");
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = CreateToken(user.Id, user.FullName, user.Email, user.Role.ToString(), expiresAtUtc);

        var response = new LoginResponse(
            token,
            expiresAtUtc,
            new AuthUserDto(user.Id, user.FullName, user.Email, user.Role)
        );

        return ApiResponse<LoginResponse>.Ok(response, "Inicio de sesión completado correctamente.");
    }

    private string CreateToken(Guid userId, string fullName, string email, string role, DateTime expiresAtUtc)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey no está configurado.");

        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer no está configurado.");

        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience no está configurado.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
