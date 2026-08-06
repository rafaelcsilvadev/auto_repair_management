using FluentValidation;

namespace AutoRepairManagement.API.Features.ServiceOrder.DTOs;

public record ServiceOrderDto(
        string Description,
        decimal Price,
        string Status,
        DateTime EndDate,
        Guid ClientId,
        Guid VehicleId);

public class ServiceOrderValidator : AbstractValidator<ServiceOrderDto>
{
    public ServiceOrderValidator()
    {
        RuleFor(s => s.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters");
        RuleFor(s => s.Price)
            .NotEmpty()
            .WithMessage("Price is required")
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");
        RuleFor(s => s.Status)
            .NotEmpty()
            .WithMessage("Status is required");
        RuleFor(s => s.ClientId)
            .NotEmpty()
            .WithMessage("ClientId is required");
        RuleFor(s => s.VehicleId)
            .NotEmpty()
            .WithMessage("VehicleId is required");
    }
}