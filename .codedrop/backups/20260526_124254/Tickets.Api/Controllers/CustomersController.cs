using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Application.Common;
using Tickets.Api.Application.Customers;

namespace Tickets.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<CustomerSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CustomerSummaryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await customerService.GetAllAsync(cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await customerService.GetByIdAsync(id, cancellationToken);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CustomerDetailDto>>> Create(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await customerService.CreateAsync(request, cancellationToken);

        if (!response.Success || response.Data is null)
        {
            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CustomerDetailDto>>> Update(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await customerService.UpdateAsync(id, request, cancellationToken);

        if (!response.Success)
        {
            if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }
}
