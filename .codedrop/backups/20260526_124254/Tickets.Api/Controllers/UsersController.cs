using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Application.Common;
using Tickets.Api.Application.Users;

namespace Tickets.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<UserSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UserSummaryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var response = await userService.GetAllAsync(cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserSummaryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserSummaryDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await userService.GetByIdAsync(id, cancellationToken);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserSummaryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserSummaryDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UserSummaryDto>>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userService.CreateAsync(request, cancellationToken);

        if (!response.Success || response.Data is null)
        {
            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserSummaryDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserSummaryDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserSummaryDto>>> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var response = await userService.UpdateAsync(id, request, cancellationToken);

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
