using FluentValidation;

namespace AutoRepairManagement.API.Features.Vehicle.DTOs;

public record VehicleDto(string Plate, string Model, int Year, int Kilometers, Guid ClientId);

public class VehicleValidator : AbstractValidator<VehicleDto>
{
    public VehicleValidator()
    {
        RuleFor(t => t.Plate)
            .NotEmpty()
            .WithMessage("Plate cannot be empty")
            .MaximumLength(8)
            .WithMessage("Plate must be 8 digits long");
        RuleFor(t => t.Model)
            .NotEmpty()
            .WithMessage("Model cannot be empty")
            .MaximumLength(50)
            .WithMessage("Model must be 50 digits long");
        RuleFor(t => t.Year)
            .GreaterThan(0)
            .WithMessage("Year must be greater than 0")
            .LessThan(DateTime.UtcNow.Year + 1);
        RuleFor(t => t.Kilometers)
            .GreaterThan(0)
            .WithMessage("Kilometers must be greater than 0");
        RuleFor(t => t.ClientId)
            .NotEmpty()
            .WithMessage("ClientId cannot be empty");
    }
}