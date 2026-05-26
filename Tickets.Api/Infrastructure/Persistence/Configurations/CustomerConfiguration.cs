using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tickets.Api.Domain.Entities;

namespace Tickets.Api.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(customer => customer.Email)
            .HasMaxLength(180)
            .IsRequired();

        builder.HasIndex(customer => customer.Email);

        builder.Property(customer => customer.Phone)
            .HasMaxLength(40);

        builder.Property(customer => customer.CompanyName)
            .HasMaxLength(180);

        builder.Property(customer => customer.Notes)
            .HasMaxLength(1000);

        builder.Property(customer => customer.CreatedAtUtc)
            .IsRequired();

        builder.Property(customer => customer.IsActive)
            .IsRequired();

        builder.HasMany(customer => customer.Tickets)
            .WithOne(ticket => ticket.Customer)
            .HasForeignKey(ticket => ticket.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
