using Microsoft.EntityFrameworkCore;
using Tickets.Api.Application.Common;
using Tickets.Api.Domain.Entities;
using Tickets.Api.Domain.Enums;
using Tickets.Api.Infrastructure.Persistence;

namespace Tickets.Api.Application.Tickets;

public class TicketService(TicketsDbContext dbContext) : ITicketService
{
    public async Task<ApiResponse<PagedResponse<TicketSummaryDto>>> GetAllAsync(
        TicketQueryParameters queryParameters,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(queryParameters.Page, 1);
        var pageSize = Math.Clamp(queryParameters.PageSize, 1, 50);

        var query = dbContext.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.Customer)
            .Include(ticket => ticket.AssignedToUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryParameters.Search))
        {
            var search = queryParameters.Search.Trim();

            query = query.Where(ticket =>
                ticket.Title.Contains(search) ||
                ticket.Description.Contains(search) ||
                (ticket.Customer != null && ticket.Customer.Name.Contains(search)) ||
                (ticket.AssignedToUser != null && ticket.AssignedToUser.FullName.Contains(search)));
        }

        if (queryParameters.Status.HasValue)
        {
            query = query.Where(ticket => ticket.Status == queryParameters.Status.Value);
        }

        if (queryParameters.Priority.HasValue)
        {
            query = query.Where(ticket => ticket.Priority == queryParameters.Priority.Value);
        }

        if (queryParameters.CustomerId.HasValue)
        {
            query = query.Where(ticket => ticket.CustomerId == queryParameters.CustomerId.Value);
        }

        if (queryParameters.AssignedToUserId.HasValue)
        {
            query = query.Where(ticket => ticket.AssignedToUserId == queryParameters.AssignedToUserId.Value);
        }

        query = ApplySorting(query, queryParameters.SortBy, queryParameters.SortDirection);

        var totalItems = await query.CountAsync(cancellationToken);

        var tickets = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ticket => new TicketSummaryDto(
                ticket.Id,
                ticket.Title,
                ticket.Status,
                ticket.Priority,
                ticket.Customer != null ? ticket.Customer.Name : "Cliente no disponible",
                ticket.AssignedToUser != null ? ticket.AssignedToUser.FullName : null,
                ticket.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        var pagedResponse = PagedResponse<TicketSummaryDto>.Create(tickets, page, pageSize, totalItems);

        return ApiResponse<PagedResponse<TicketSummaryDto>>.Ok(
            pagedResponse,
            "Tickets obtenidos correctamente."
        );
    }

    public async Task<ApiResponse<TicketDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await GetTicketQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (ticket is null)
        {
            return ApiResponse<TicketDetailDto>.Fail("No se encontró el ticket solicitado.");
        }

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(ticket), "Ticket obtenido correctamente.");
    }

    public async Task<ApiResponse<TicketDetailDto>> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateCreate(request);

        if (errors.Count > 0)
        {
            return ApiResponse<TicketDetailDto>.Fail("La validación del ticket falló.", errors);
        }

        var customerExists = await dbContext.Customers
            .AnyAsync(customer => customer.Id == request.CustomerId && customer.IsActive, cancellationToken);

        if (!customerExists)
        {
            return ApiResponse<TicketDetailDto>.Fail("El cliente no existe o está inactivo.");
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
                return ApiResponse<TicketDetailDto>.Fail("El usuario asignado debe ser un administrador o agente activo.");
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

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(createdTicket), "Ticket creado correctamente.");
    }

    public async Task<ApiResponse<TicketDetailDto>> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateUpdate(request);

        if (errors.Count > 0)
        {
            return ApiResponse<TicketDetailDto>.Fail("La validación del ticket falló.", errors);
        }

        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (ticket is null)
        {
            return ApiResponse<TicketDetailDto>.Fail("No se encontró el ticket solicitado.");
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
                return ApiResponse<TicketDetailDto>.Fail("El usuario asignado debe ser un administrador o agente activo.");
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

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(updatedTicket), "Ticket actualizado correctamente.");
    }

    public async Task<ApiResponse<TicketDetailDto>> AddCommentAsync(Guid ticketId, AddTicketCommentRequest request, CancellationToken cancellationToken)
    {
        var errors = ValidateComment(request);

        if (errors.Count > 0)
        {
            return ApiResponse<TicketDetailDto>.Fail("La validación del comentario falló.", errors);
        }

        var ticketExists = await dbContext.Tickets
            .AnyAsync(ticket => ticket.Id == ticketId, cancellationToken);

        if (!ticketExists)
        {
            return ApiResponse<TicketDetailDto>.Fail("No se encontró el ticket solicitado.");
        }

        var authorExists = await dbContext.Users
            .AnyAsync(user => user.Id == request.AuthorUserId && user.IsActive, cancellationToken);

        if (!authorExists)
        {
            return ApiResponse<TicketDetailDto>.Fail("El usuario autor no existe o está inactivo.");
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

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(updatedTicket), "Comentario agregado correctamente.");
    }

    public async Task<ApiResponse<TicketDetailDto>> ChangeStatusAsync(Guid ticketId, ChangeTicketStatusRequest request, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(item => item.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            return ApiResponse<TicketDetailDto>.Fail("No se encontró el ticket solicitado.");
        }

        ApplyStatus(ticket, request.Status);

        await dbContext.SaveChangesAsync(cancellationToken);

        var updatedTicket = await GetTicketQuery()
            .AsNoTracking()
            .FirstAsync(item => item.Id == ticketId, cancellationToken);

        return ApiResponse<TicketDetailDto>.Ok(MapToDetailDto(updatedTicket), "Estado del ticket actualizado correctamente.");
    }

    private static IQueryable<SupportTicket> ApplySorting(
        IQueryable<SupportTicket> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => descending
                ? query.OrderByDescending(ticket => ticket.Title)
                : query.OrderBy(ticket => ticket.Title),

            "priority" => descending
                ? query.OrderByDescending(ticket => ticket.Priority)
                : query.OrderBy(ticket => ticket.Priority),

            "status" => descending
                ? query.OrderByDescending(ticket => ticket.Status)
                : query.OrderBy(ticket => ticket.Status),

            _ => descending
                ? query.OrderByDescending(ticket => ticket.CreatedAtUtc)
                : query.OrderBy(ticket => ticket.CreatedAtUtc)
        };
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
            ticket.Customer?.Name ?? "Cliente no disponible",
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
                    comment.AuthorUser?.FullName ?? "Usuario no disponible",
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

        ValidateText(request.Title, "El título", 180, errors);
        ValidateText(request.Description, "La descripción", 3000, errors);

        if (request.CustomerId == Guid.Empty)
        {
            errors.Add("El cliente es obligatorio.");
        }

        return errors;
    }

    private static List<string> ValidateUpdate(UpdateTicketRequest request)
    {
        var errors = new List<string>();

        ValidateText(request.Title, "El título", 180, errors);
        ValidateText(request.Description, "La descripción", 3000, errors);

        return errors;
    }

    private static List<string> ValidateComment(AddTicketCommentRequest request)
    {
        var errors = new List<string>();

        if (request.AuthorUserId == Guid.Empty)
        {
            errors.Add("El usuario autor es obligatorio.");
        }

        ValidateText(request.Message, "El mensaje", 2000, errors);

        return errors;
    }

    private static void ValidateText(string value, string fieldName, int maxLength, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{fieldName} es obligatorio.");
            return;
        }

        if (value.Length > maxLength)
        {
            errors.Add($"{fieldName} no puede exceder {maxLength} caracteres.");
        }
    }
}
