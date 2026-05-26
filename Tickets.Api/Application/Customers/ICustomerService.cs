using Tickets.Api.Application.Common;

namespace Tickets.Api.Application.Customers;

public interface ICustomerService
{
    Task<ApiResponse<IReadOnlyCollection<CustomerSummaryDto>>> GetAllAsync(CancellationToken cancellationToken);

    Task<ApiResponse<CustomerDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ApiResponse<CustomerDetailDto>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken);

    Task<ApiResponse<CustomerDetailDto>> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken);
}
