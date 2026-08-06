namespace AutoRepairManagement.API.Features.Vehicle.Mappers;

public record VehicleMapper(
    Guid Id, 
    string Plate, 
    string Model, 
    int Year, 
    int Kilometers, 
    string ClientName);