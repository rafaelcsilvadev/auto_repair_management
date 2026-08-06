namespace AutoRepairManagement.API.Features.ServiceOrder.Mappers;

public record ServiceOrderMapper(
     Guid Id,
     string ClientName,
     string Plate,
     string Description,
     decimal Price,
     DateTime StartDate,
     DateTime EndDate,
     string Status);