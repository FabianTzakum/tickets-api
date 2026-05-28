using Microsoft.EntityFrameworkCore;
using Tickets.Api.Domain.Entities;

namespace Tickets.Api.Infrastructure.Persistence;

public class TicketsDbContext(DbContextOptions<TicketsDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<SupportTicket> Tickets => Set<SupportTicket>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    public DbSet<TicketHistory> TicketHistory => Set<TicketHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketsDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditValues();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditValues();

        return base.SaveChanges();
    }

    private void ApplyAuditValues()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(entry =>
                entry.Entity is Domain.Common.AuditableEntity &&
                (entry.State == EntityState.Added || entry.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.AuditableEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAtUtc = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
    }
}
