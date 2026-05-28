using Microsoft.EntityFrameworkCore;
using Tickets.Api.Application.Common;
using Tickets.Api.Domain.Entities;
using Tickets.Api.Infrastructure.Persistence;

namespace Tickets.Api.Application.Customers;

public class CustomerService(TicketsDbContext dbContext) : ICustomerService
{
    public async Task<ApiResponse<IReadOnlyCollection<CustomerSummaryDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var customers = await dbContext.Customers
            .AsNoTracking()
            .OrderByDescending(customer => customer.CreatedAtUtc)
            .Select(customer => new CustomerSummaryDto(
                customer.Id,
                customer.Name,
                customer.Email,
                customer.Phone,
                customer.CompanyName,
                customer.IsActive,
                customer.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CustomerSummaryDto>>.Ok(
            customers,
            "Clientes obtenidos correctamente."
        );
    }

    public async Task<ApiResponse<CustomerDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (customer is null)
        {
            return ApiResponse<CustomerDetailDto>.Fail("No se encontró el cliente solicitado.");
        }

        return ApiResponse<CustomerDetailDto>.Ok(
            MapToDetailDto(customer),
            "Cliente obtenido correctamente."
        );
    }

    public async Task<ApiResponse<CustomerDetailDto>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateCreateOrUpdate(request.Name, request.Email);

        if (errors.Count > 0)
        {
            return ApiResponse<CustomerDetailDto>.Fail("La validación del cliente falló.", errors);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Customers
            .AnyAsync(customer => customer.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<CustomerDetailDto>.Fail(
                "El correo del cliente ya existe.",
                ["El correo debe ser único."]
            );
        }

        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            Phone = NormalizeNullable(request.Phone),
            CompanyName = NormalizeNullable(request.CompanyName),
            Notes = NormalizeNullable(request.Notes)
        };

        dbContext.Customers.Add(customer);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CustomerDetailDto>.Ok(
            MapToDetailDto(customer),
            "Cliente creado correctamente."
        );
    }

    public async Task<ApiResponse<CustomerDetailDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateCreateOrUpdate(request.Name, request.Email);

        if (errors.Count > 0)
        {
            return ApiResponse<CustomerDetailDto>.Fail("La validación del cliente falló.", errors);
        }

        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (customer is null)
        {
            return ApiResponse<CustomerDetailDto>.Fail("No se encontró el cliente solicitado.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Customers
            .AnyAsync(item => item.Id != id && item.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<CustomerDetailDto>.Fail(
                "El correo del cliente ya existe.",
                ["El correo debe ser único."]
            );
        }

        customer.Name = request.Name.Trim();
        customer.Email = normalizedEmail;
        customer.Phone = NormalizeNullable(request.Phone);
        customer.CompanyName = NormalizeNullable(request.CompanyName);
        customer.Notes = NormalizeNullable(request.Notes);
        customer.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CustomerDetailDto>.Ok(
            MapToDetailDto(customer),
            "Cliente actualizado correctamente."
        );
    }

    private static CustomerDetailDto MapToDetailDto(Customer customer)
    {
        return new CustomerDetailDto(
            customer.Id,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CompanyName,
            customer.Notes,
            customer.IsActive,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc
        );
    }

    private static List<string> ValidateCreateOrUpdate(string name, string email)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("El nombre es obligatorio.");
        }

        if (name.Length > 160)
        {
            errors.Add("El nombre no puede exceder 160 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("El correo es obligatorio.");
        }

        if (!email.Contains('@', StringComparison.Ordinal) || email.Length > 180)
        {
            errors.Add("El correo no tiene un formato válido.");
        }

        return errors;
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
