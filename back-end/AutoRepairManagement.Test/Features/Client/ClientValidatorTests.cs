using AutoRepairManagement.API.Features.Client.DTOs;

namespace AutoRepairManagement.Test.Features.Client;

public class ClientValidatorTests
{
    private readonly ClientValidator _validator = new();

    [Fact]
    public void Validate_WhenPayloadIsValid_HasNoErrors()
    {
        // Arrange
        var dto = new ClientDto("Maria Silva", "maria@example.com");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "maria@example.com")]
    [InlineData("Maria Silva", "")]
    [InlineData("Maria Silva", "not-an-email")]
    public void Validate_WhenRequiredFieldIsMissingOrInvalid_HasErrors(string name, string email)
    {
        // Arrange
        var dto = new ClientDto(name, email);

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNameExceedsMaximumLength_HasError()
    {
        // Arrange
        var dto = new ClientDto(new string('a', 51), "maria@example.com");

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name cannot exceed 50 characters");
    }
}
