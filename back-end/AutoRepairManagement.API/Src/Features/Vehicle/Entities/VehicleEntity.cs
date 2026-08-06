using AutoRepairManagement.API.Features.Client.Entities;
using AutoRepairManagement.API.Features.ServiceOrder.Entities;

namespace AutoRepairManagement.API.Features.Vehicle.Entities;

public class VehicleEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Plate  { get; set; }
    public required string Model { get; set; }
    public required int Year { get; set; }
    public required int Kilometers { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Guid ClientId { get; set; }
    public ClientEntity Client { get; set; } = null!;

    public ICollection<ServiceOrderEntity> ServiceOrder { get; set; } = null!;
}