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

        return ApiResponse<IReadOnlyCollection<CustomerSummaryDto>>.Ok(customers);
    }

    public async Task<ApiResponse<CustomerDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (customer is null)
        {
            return ApiResponse<CustomerDetailDto>.Fail("Customer was not found.");
        }

        return ApiResponse<CustomerDetailDto>.Ok(MapToDetailDto(customer));
    }

    public async Task<ApiResponse<CustomerDetailDto>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateCreateOrUpdate(request.Name, request.Email);

        if (errors.Count > 0)
        {
            return ApiResponse<CustomerDetailDto>.Fail("Customer validation failed.", errors);
        }

        var emailExists = await dbContext.Customers
            .AnyAsync(customer => customer.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<CustomerDetailDto>.Fail("Customer email already exists.", ["Email must be unique."]);
        }

        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = NormalizeNullable(request.Phone),
            CompanyName = NormalizeNullable(request.CompanyName),
            Notes = NormalizeNullable(request.Notes)
        };

        dbContext.Customers.Add(customer);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CustomerDetailDto>.Ok(MapToDetailDto(customer), "Customer created successfully.");
    }

    public async Task<ApiResponse<CustomerDetailDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateCreateOrUpdate(request.Name, request.Email);

        if (errors.Count > 0)
        {
            return ApiResponse<CustomerDetailDto>.Fail("Customer validation failed.", errors);
        }

        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (customer is null)
        {
            return ApiResponse<CustomerDetailDto>.Fail("Customer was not found.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Customers
            .AnyAsync(item => item.Id != id && item.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            return ApiResponse<CustomerDetailDto>.Fail("Customer email already exists.", ["Email must be unique."]);
        }

        customer.Name = request.Name.Trim();
        customer.Email = normalizedEmail;
        customer.Phone = NormalizeNullable(request.Phone);
        customer.CompanyName = NormalizeNullable(request.CompanyName);
        customer.Notes = NormalizeNullable(request.Notes);
        customer.IsActive = request.IsActive;

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CustomerDetailDto>.Ok(MapToDetailDto(customer), "Customer updated successfully.");
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
            errors.Add("Name is required.");
        }

        if (name.Length > 160)
        {
            errors.Add("Name cannot exceed 160 characters.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required.");
        }

        if (!email.Contains('@', StringComparison.Ordinal) || email.Length > 180)
        {
            errors.Add("Email is not valid.");
        }

        return errors;
    }

    private static string? NormalizeNullable(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
