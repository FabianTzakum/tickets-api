using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickets.Api.Application.Common;
using Tickets.Api.Application.Tickets;

namespace Tickets.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize(Roles = "Admin,SupportAgent")]
public class TicketsController(ITicketService ticketService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TicketSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResponse<TicketSummaryDto>>>> GetAll(
        [FromQuery] TicketQueryParameters queryParameters,
        CancellationToken cancellationToken)
    {
        var response = await ticketService.GetAllAsync(queryParameters, cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await ticketService.GetByIdAsync(id, cancellationToken);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TicketDetailDto>>> Create(
        CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var response = await ticketService.CreateAsync(request, cancellationToken);

        if (!response.Success || response.Data is null)
        {
            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetById), new { id = response.Data.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketDetailDto>>> Update(
        Guid id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var response = await ticketService.UpdateAsync(id, request, cancellationToken);

        if (!response.Success)
        {
            if (response.Message.Contains("no se encontró", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketDetailDto>>> AddComment(
        Guid id,
        AddTicketCommentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await ticketService.AddCommentAsync(id, request, cancellationToken);

        if (!response.Success)
        {
            if (response.Message.Contains("no se encontró", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TicketDetailDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TicketDetailDto>>> ChangeStatus(
        Guid id,
        ChangeTicketStatusRequest request,
        CancellationToken cancellationToken)
    {
        var response = await ticketService.ChangeStatusAsync(id, request, cancellationToken);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }
}
