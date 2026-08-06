using AutoRepairManagement.API.Features.ServiceOrder.DTOs;

namespace AutoRepairManagement.Test.Features.ServiceOrder;

public class ServiceOrderValidatorTests
{
    private readonly ServiceOrderValidator _validator = new();

    private static ServiceOrderDto ValidDto() =>
        new("Oil change", 150m, "Open", DateTime.UtcNow.AddDays(1), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Validate_WhenPayloadIsValid_HasNoErrors()
    {
        // Arrange
        var dto = ValidDto();

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Description = "" };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ServiceOrderDto.Description));
    }

    [Fact]
    public void Validate_WhenDescriptionExceedsMaximumLength_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Description = new string('a', 201) };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ServiceOrderDto.Description));
    }

    [Fact]
    public void Validate_WhenPriceIsZero_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Price = 0m };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ServiceOrderDto.Price));
    }

    [Fact]
    public void Validate_WhenPriceIsNegative_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Price = -50m };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ServiceOrderDto.Price));
    }

    [Fact]
    public void Validate_WhenStatusIsEmpty_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Status = "" };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ServiceOrderDto.Status));
    }

    [Fact]
    public void Validate_WhenClientIdIsEmpty_HasError()
    {
        // Arrange
        var dto = ValidDto() with { ClientId = Guid.Empty };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ServiceOrderDto.ClientId));
    }

    [Fact]
    public void Validate_WhenVehicleIdIsEmpty_HasError()
    {
        // Arrange
        var dto = ValidDto() with { VehicleId = Guid.Empty };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ServiceOrderDto.VehicleId));
    }
}
