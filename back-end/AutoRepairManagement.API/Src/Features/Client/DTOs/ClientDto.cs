using FluentValidation;

namespace AutoRepairManagement.API.Features.Client.DTOs;

public record ClientDto(string Name, string Email);

public class ClientValidator : AbstractValidator<ClientDto>
{
    public ClientValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty() 
            .WithMessage("Name is required")
            .MaximumLength(50)
            .WithMessage("Name cannot exceed 50 characters");
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is invalid");
    }
}
