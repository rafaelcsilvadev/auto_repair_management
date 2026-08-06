using AutoRepairManagement.API.Features.ServiceOrder.Entities;
using AutoRepairManagement.API.Features.Vehicle.Entities;

namespace AutoRepairManagement.API.Features.Client.Entities;

public class ClientEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<VehicleEntity> Vehicles { get; init; } = new List<VehicleEntity>();
    
    public ICollection<ServiceOrderEntity> ServiceOrder { get; set; } = null!;
}