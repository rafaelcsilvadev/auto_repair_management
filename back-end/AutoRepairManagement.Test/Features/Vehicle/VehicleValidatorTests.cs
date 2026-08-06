using AutoRepairManagement.API.Features.Vehicle.DTOs;

namespace AutoRepairManagement.Test.Features.Vehicle;

public class VehicleValidatorTests
{
    private readonly VehicleValidator _validator = new();

    private static VehicleDto ValidDto() => new("ABC1234", "Onix", 2020, 1000, Guid.NewGuid());

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
    public void Validate_WhenPlateIsEmpty_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Plate = "" };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VehicleDto.Plate));
    }

    [Fact]
    public void Validate_WhenPlateExceedsMaximumLength_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Plate = "ABCD12345" };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VehicleDto.Plate));
    }

    [Fact]
    public void Validate_WhenModelIsEmpty_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Model = "" };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VehicleDto.Model));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenYearIsNotGreaterThanZero_HasError(int year)
    {
        // Arrange
        var dto = ValidDto() with { Year = year };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VehicleDto.Year));
    }

    [Fact]
    public void Validate_WhenYearIsInTheFuture_HasError()
    {
        // Arrange
        var dto = ValidDto() with { Year = DateTime.UtcNow.Year + 2 };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VehicleDto.Year));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenKilometersIsNotGreaterThanZero_HasError(int kilometers)
    {
        // Arrange
        var dto = ValidDto() with { Kilometers = kilometers };

        // Act
        var result = _validator.Validate(dto);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VehicleDto.Kilometers));
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
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VehicleDto.ClientId));
    }
}
