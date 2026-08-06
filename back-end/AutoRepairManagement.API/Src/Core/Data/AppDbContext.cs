using AutoRepairManagement.API.Features.Client.Entities;
using AutoRepairManagement.API.Features.ServiceOrder.Entities;
using AutoRepairManagement.API.Features.Vehicle.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairManagement.API.Core.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
      public DbSet<ClientEntity> Clients => Set<ClientEntity>();
      public DbSet<VehicleEntity> Vehicles => Set<VehicleEntity>();
      public DbSet<ServiceOrderEntity>  ServiceOrders => Set<ServiceOrderEntity>();

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ClientEntity>(entity =>
            {
                  entity.ToTable("Clients");
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(50);
                  entity.Property(e => e.Email)
                        .IsRequired()
                        .HasMaxLength(50);
                  entity.HasIndex(e => e.Email)
                        .IsUnique()
                        .HasFilter("[DeletedAt] IS NULL");
            });

            modelBuilder.Entity<VehicleEntity>(entity =>
            {
                  entity.ToTable("Vehicles");
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.Plate)
                        .IsRequired()
                        .HasMaxLength(8);
                  entity.HasIndex(e => e.Plate)
                        .IsUnique()
                        .HasFilter("[DeletedAt] IS NULL");
                  entity.Property(e => e.Model)
                        .IsRequired()
                        .HasMaxLength(50);
                  entity.Property(e => e.Year)
                        .IsRequired();
                  entity.Property(e => e.Kilometers)
                        .IsRequired();
                  entity.Property(e => e.ClientId)
                        .IsRequired();
            });

            modelBuilder.Entity<ServiceOrderEntity>(entity =>
            {
                  entity.ToTable("ServiceOrders");
                  entity.HasKey(e => e.Id);
                  entity.Property(e => e.ServiceOrder);
                  entity.HasIndex(e => e.ServiceOrder)
                        .IsUnique();
                  entity.Property(e => e.Description)
                        .IsRequired();
                  entity.Property(e => e.Price)
                        .IsRequired();
                  entity.Property(e => e.StartDate)
                        .IsRequired();
                  entity.Property(e => e.EndDate)
                        .IsRequired();
                  entity.Property(e => e.Status)
                        .IsRequired();
                  entity.Property(e => e.VehicleId)
                        .IsRequired();
                  entity.Property(e => e.ClientId)
                        .IsRequired();
            });
      }
}
