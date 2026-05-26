using Microsoft.EntityFrameworkCore;
using Tickets.Api.Domain.Entities;
using Tickets.Api.Domain.Enums;

namespace Tickets.Api.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TicketsDbContext>();

        await dbContext.Database.MigrateAsync();

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var admin = new AppUser
        {
            FullName = "Administrador Demo",
            Email = "admin@tickets.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin12345"),
            Role = UserRole.Admin
        };

        var agent = new AppUser
        {
            FullName = "Agente Soporte Demo",
            Email = "agent@tickets.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent12345"),
            Role = UserRole.SupportAgent
        };

        var customerUser = new AppUser
        {
            FullName = "Cliente Usuario Demo",
            Email = "client@tickets.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Client12345"),
            Role = UserRole.Client
        };

        var customerOne = new Customer
        {
            Name = "Comercial del Sureste",
            Email = "contacto@comercialsureste.local",
            Phone = "9991234567",
            CompanyName = "Comercial del Sureste",
            Notes = "Cliente demo para pruebas de soporte técnico."
        };

        var customerTwo = new Customer
        {
            Name = "Servicios Peninsulares",
            Email = "soporte@peninsulares.local",
            Phone = "9997654321",
            CompanyName = "Servicios Peninsulares",
            Notes = "Cliente demo con tickets de prioridad media y alta."
        };

        dbContext.Users.AddRange(admin, agent, customerUser);
        dbContext.Customers.AddRange(customerOne, customerTwo);

        await dbContext.SaveChangesAsync();

        var ticketOne = new SupportTicket
        {
            Title = "No puedo acceder al sistema",
            Description = "El usuario reporta que no puede iniciar sesión desde la mañana.",
            Priority = TicketPriority.High,
            Status = TicketStatus.Open,
            CustomerId = customerOne.Id,
            AssignedToUserId = agent.Id
        };

        var ticketTwo = new SupportTicket
        {
            Title = "Error al generar reporte mensual",
            Description = "El reporte mensual muestra información incompleta al exportar.",
            Priority = TicketPriority.Medium,
            Status = TicketStatus.InProgress,
            CustomerId = customerTwo.Id,
            AssignedToUserId = agent.Id
        };

        var ticketThree = new SupportTicket
        {
            Title = "Solicitud de actualización de datos",
            Description = "El cliente solicita actualizar información de contacto.",
            Priority = TicketPriority.Low,
            Status = TicketStatus.Resolved,
            CustomerId = customerOne.Id,
            AssignedToUserId = admin.Id,
            ResolvedAtUtc = DateTime.UtcNow
        };

        dbContext.Tickets.AddRange(ticketOne, ticketTwo, ticketThree);

        await dbContext.SaveChangesAsync();

        var comments = new List<TicketComment>
        {
            new()
            {
                TicketId = ticketOne.Id,
                AuthorUserId = agent.Id,
                Message = "Se inició la revisión del acceso del usuario.",
                IsInternal = false
            },
            new()
            {
                TicketId = ticketTwo.Id,
                AuthorUserId = agent.Id,
                Message = "Se detectó que el reporte falla al procesar registros incompletos.",
                IsInternal = true
            },
            new()
            {
                TicketId = ticketThree.Id,
                AuthorUserId = admin.Id,
                Message = "La información fue actualizada correctamente.",
                IsInternal = false
            }
        };

        dbContext.TicketComments.AddRange(comments);

        await dbContext.SaveChangesAsync();
    }
}
