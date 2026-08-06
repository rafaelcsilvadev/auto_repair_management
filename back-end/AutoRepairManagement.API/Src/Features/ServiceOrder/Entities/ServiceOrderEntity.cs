using AutoRepairManagement.API.Features.Client.Entities;
using AutoRepairManagement.API.Features.Vehicle.Entities;

namespace AutoRepairManagement.API.Features.ServiceOrder.Entities;

public class ServiceOrderEntity
{
    public Guid Id { get; init; } =  Guid.NewGuid();  
    public int ServiceOrder { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; init; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public required string  Status { get; set; }
    public DateTime CreatedAt { get; init; }  = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Guid VehicleId { get; set; }
    public VehicleEntity Vehicle { get; init; } = null!;

    public Guid ClientId { get; set; }
    public ClientEntity Client { get; set; } = null!;
    
}