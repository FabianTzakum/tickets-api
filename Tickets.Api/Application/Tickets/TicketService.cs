using Microsoft.EntityFrameworkCore;
using Tickets.Api.Application.Common;
using Tickets.Api.Domain.Entities;
using Tickets.Api.Domain.Enums;
using Tickets.Api.Infrastructure.Persistence;

namespace Tickets.Api.Application.Tickets;

public class TicketService(TicketsDbContext dbContext) : ITicketService
{
    public async Task<ApiResponse<IReadOnlyCollection<TicketSummaryDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var tickets = await dbContext.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.Customer)
            .Include(ticket => ticket.AssignedToUser)
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .Select(ticket => new TicketSummaryDto(
                ticket.Id,
                ticket.Title,
                ticket.Status,
                ticket.Priority,
                ticket.Customer != null ? ticket.Customer.Name : "Unknown customer",
                ticket.AssignedToUser != null ? ticket.AssignedToUser.FullName : null,
                ticket.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<TicketSummaryDto>>.Ok(tickets);
    }

    public async Task<ApiResponse<TicketDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await GetTicketQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (ticket is null)
        {
            return ApiResponse<TicketDetailDto>.Fail("Ticket was not found.");
        }

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(ticket));
    }

    public async Task<ApiResponse<TicketDetailDto>> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateCreate(request);

        if (errors.Count > 0)
        {
            return ApiResponse<TicketDetailDto>.Fail("Ticket validation failed.", errors);
        }

        var customerExists = await dbContext.Customers
            .AnyAsync(customer => customer.Id == request.CustomerId && customer.IsActive, cancellationToken);

        if (!customerExists)
        {
            return ApiResponse<TicketDetailDto>.Fail("Customer was not found or is inactive.");
        }

        if (request.AssignedToUserId.HasValue)
        {
            var agentExists = await dbContext.Users
                .AnyAsync(user =>
                    user.Id == request.AssignedToUserId.Value &&
                    user.IsActive &&
                    (user.Role == UserRole.Admin || user.Role == UserRole.SupportAgent),
                    cancellationToken);

            if (!agentExists)
            {
                return ApiResponse<TicketDetailDto>.Fail("Assigned user must be an active admin or support agent.");
            }
        }

        var ticket = new SupportTicket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CustomerId = request.CustomerId,
            AssignedToUserId = request.AssignedToUserId
        };

        dbContext.Tickets.Add(ticket);

        await dbContext.SaveChangesAsync(cancellationToken);

        var createdTicket = await GetTicketQuery()
            .AsNoTracking()
            .FirstAsync(item => item.Id == ticket.Id, cancellationToken);

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(createdTicket), "Ticket created successfully.");
    }

    public async Task<ApiResponse<TicketDetailDto>> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateUpdate(request);

        if (errors.Count > 0)
        {
            return ApiResponse<TicketDetailDto>.Fail("Ticket validation failed.", errors);
        }

        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (ticket is null)
        {
            return ApiResponse<TicketDetailDto>.Fail("Ticket was not found.");
        }

        if (request.AssignedToUserId.HasValue)
        {
            var agentExists = await dbContext.Users
                .AnyAsync(user =>
                    user.Id == request.AssignedToUserId.Value &&
                    user.IsActive &&
                    (user.Role == UserRole.Admin || user.Role == UserRole.SupportAgent),
                    cancellationToken);

            if (!agentExists)
            {
                return ApiResponse<TicketDetailDto>.Fail("Assigned user must be an active admin or support agent.");
            }
        }

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Priority = request.Priority;
        ticket.AssignedToUserId = request.AssignedToUserId;
        ticket.IsActive = request.IsActive;

        ApplyStatus(ticket, request.Status);

        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedTicket = await GetTicketQuery()
            .AsNoTracking()
            .FirstAsync(item => item.Id == id, cancellationToken);

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(updatedTicket), "Ticket updated successfully.");
    }

    public async Task<ApiResponse<TicketDetailDto>> AddCommentAsync(Guid ticketId, AddTicketCommentRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateComment(request);

        if (errors.Count > 0)
        {
            return ApiResponse<TicketDetailDto>.Fail("Comment validation failed.", errors);
        }

        var ticketExists = await dbContext.Tickets
            .AnyAsync(ticket => ticket.Id == ticketId, cancellationToken);

        if (!ticketExists)
        {
            return ApiResponse<TicketDetailDto>.Fail("Ticket was not found.");
        }

        var authorExists = await dbContext.Users
            .AnyAsync(user => user.Id == request.AuthorUserId && user.IsActive, cancellationToken);

        if (!authorExists)
        {
            return ApiResponse<TicketDetailDto>.Fail("Author user was not found or is inactive.");
        }

        var comment = new TicketComment
        {
            TicketId = ticketId,
            AuthorUserId = request.AuthorUserId,
            Message = request.Message.Trim(),
            IsInternal = request.IsInternal
        };

        dbContext.TicketComments.Add(comment);

        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedTicket = await GetTicketQuery()
            .AsNoTracking()
            .FirstAsync(item => item.Id == ticketId, cancellationToken);

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(updatedTicket), "Comment added successfully.");
    }

    public async Task<ApiResponse<TicketDetailDto>> ChangeStatusAsync(Guid ticketId, ChangeTicketStatusRequest request, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(item => item.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return ApiResponse<TicketDetailDto>.Fail("Ticket was not found.");
        }

        ApplyStatus(ticket, request.Status);

        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedTicket = await GetTicketQuery()
            .AsNoTracking()
            .FirstAsync(item => item.Id == ticketId, cancellationToken);

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(updatedTicket), "Ticket status updated successfully.");
    }

    private IQueryable<SupportTicket> GetTicketQuery()
    {
        return dbContext.Tickets
            .Include(ticket => ticket.Customer)
            .Include(ticket => ticket.AssignedToUser)
            .Include(ticket => ticket.Comments)
            .ThenInclude(comment => comment.AuthorUser);
    }

    private static TicketDetailDto MapToDetailDto(SupportTicket ticket)
    {
        return new TicketDetailDto(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CustomerId,
            ticket.Customer?.Name ?? "Unknown customer",
            ticket.AssignedToUserId,
            ticket.AssignedToUser?.FullName,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.ResolvedAtUtc,
            ticket.ClosedAtUtc,
            ticket.Comments
                .OrderBy(comment => comment.CreatedAtUtc)
                .Select(comment => new TicketCommentDto(
                    comment.Id,
                    comment.AuthorUserId,
                    comment.AuthorUser?.FullName ?? "Unknown user",
                    comment.Message,
                    comment.IsInternal,
                    comment.CreatedAtUtc
                ))
                .ToList()
        );
    }

    private static void ApplyStatus(SupportTicket ticket, TicketStatus status)
    {
        ticket.Status = status;

        if (status == TicketStatus.Resolved && ticket.ResolvedAtUtc is null)
        {
            ticket.ResolvedAtUtc = DateTime.UtcNow;
        }

        if (status == TicketStatus.Closed && ticket.ClosedAtUtc is null)
        {
            ticket.ClosedAtUtc = DateTime.UtcNow;
        }
    }

    private static List<string> ValidateCreate(CreateTicketRequest request)
    {
        var errors = new List<string>();

        ValidateText(request.Title, "Title", 180, errors);
        ValidateText(request.Description, "Description", 3000, errors);

        if (request.CustomerId == Guid.Empty)
        {
            errors.Add("CustomerId is required.");
        }

        return errors;
    }

    private static List<string> ValidateUpdate(UpdateTicketRequest request)
    {
        var errors = new List<string>();

        ValidateText(request.Title, "Title", 180, errors);
        ValidateText(request.Description, "Description", 3000, errors);

        return errors;
    }

    private static List<string> ValidateComment(AddTicketCommentRequest request)
    {
        var errors = new List<string>();

        if (request.AuthorUserId == Guid.Empty)
        {
            errors.Add("AuthorUserId is required.");
        }

        ValidateText(request.Message, "Message", 2000, errors);

        return errors;
    }

    private static void ValidateText(string value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} is required.");
            return;
        }

        if (value.Length > maxLength)
        {
            errors.Add($"{fieldName} cannot exceed {maxLength} characters.");
        }
    }
}
