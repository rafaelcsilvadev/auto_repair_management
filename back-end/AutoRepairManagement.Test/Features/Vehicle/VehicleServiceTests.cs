using AutoRepairManagement.API.Core.Data;
using AutoRepairManagement.API.Features.Client.Entities;
using AutoRepairManagement.API.Features.Vehicle.DTOs;
using AutoRepairManagement.API.Features.Vehicle.Entities;
using AutoRepairManagement.API.Features.Vehicle.Mappers;
using AutoRepairManagement.API.Features.Vehicle.Services;
using AutoRepairManagement.Test.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoRepairManagement.Test.Features.Vehicle;

public class VehicleServiceTests
{
    private static VehicleService CreateService(AppDbContext dbContext) =>
        new(dbContext, new VehicleValidator(), NullLogger<VehicleService>.Instance);

    private static ClientEntity CreateClient() => new() { Name = "Maria Silva", Email = "maria@example.com" };

    [Fact]
    public async Task GetVehicleByIdAsync_WhenVehicleExists_ReturnsOkWithClientName()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        var vehicle = new VehicleEntity { Plate = "ABC1234", Model = "Onix", Year = 2020, Kilometers = 1000, ClientId = client.Id };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetVehicleByIdAsync(vehicle.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var mapped = Assert.IsType<VehicleMapper>(result.Data);
        Assert.Equal("ABC1234", mapped.Plate);
        Assert.Equal("Maria Silva", mapped.ClientName);
    }

    [Fact]
    public async Task GetVehicleByIdAsync_WhenVehicleIsSoftDeleted_ReturnsOkWithNullData()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        var vehicle = new VehicleEntity
        {
            Plate = "ABC1234", Model = "Onix", Year = 2020, Kilometers = 1000,
            ClientId = client.Id, DeletedAt = DateTime.UtcNow,
        };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetVehicleByIdAsync(vehicle.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetVehiclesAsync_ReturnsPagedResultsOrderedByPlate()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        db.Context.Clients.Add(client);
        db.Context.Vehicles.AddRange(
            new VehicleEntity { Plate = "BBB2222", Model = "Onix", Year = 2020, Kilometers = 1000, ClientId = client.Id },
            new VehicleEntity { Plate = "AAA1111", Model = "Gol", Year = 2019, Kilometers = 2000, ClientId = client.Id },
            new VehicleEntity { Plate = "CCC3333", Model = "HB20", Year = 2021, Kilometers = 500, ClientId = client.Id });
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetVehiclesAsync(page: 1, pageSize: 2, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var vehicles = Assert.IsType<List<VehicleMapper>>(result.Data);
        Assert.Equal(2, vehicles.Count);
        Assert.Equal("AAA1111", vehicles[0].Plate);
        Assert.Equal("BBB2222", vehicles[1].Plate);
        Assert.Equal(2, result.PageTotal);
    }

    [Fact]
    public async Task CreateVehicleAsync_WhenPayloadIsValid_CreatesVehicleAndReturnsCreated()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        db.Context.Clients.Add(client);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new VehicleDto("ABC1234", "Onix", 2020, 1000, client.Id);

        // Act
        var result = await service.CreateVehicleAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, result.Status);
        Assert.Single(db.Context.Vehicles);
    }

    [Fact]
    public async Task CreateVehicleAsync_WhenPayloadIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var service = CreateService(db.Context);
        var dto = new VehicleDto("", "", 0, 0, Guid.Empty);

        // Act
        var result = await service.CreateVehicleAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, result.Status);
        Assert.Empty(db.Context.Vehicles);
    }

    [Fact]
    public async Task CreateVehicleAsync_WhenPlateAlreadyExists_ReturnsConflict()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(new VehicleEntity { Plate = "ABC1234", Model = "Onix", Year = 2020, Kilometers = 1000, ClientId = client.Id });
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new VehicleDto("ABC1234", "Gol", 2019, 500, client.Id);

        // Act
        var result = await service.CreateVehicleAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, result.Status);
    }

    [Fact]
    public async Task UpdateVehicleAsync_WhenVehicleExists_UpdatesFieldsAndReturnsOk()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        var vehicle = new VehicleEntity { Plate = "ABC1234", Model = "Onix", Year = 2020, Kilometers = 1000, ClientId = client.Id };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new VehicleDto("XYZ9876", "Onix", 2021, 1500, client.Id);

        // Act
        var result = await service.UpdateVehicleAsync(vehicle.Id, dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var updated = await db.Context.Vehicles.FindAsync(vehicle.Id);
        Assert.Equal("XYZ9876", updated!.Plate);
        Assert.Equal(1500, updated.Kilometers);
    }

    [Fact]
    public async Task UpdateVehicleAsync_WhenVehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        db.Context.Clients.Add(client);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new VehicleDto("XYZ9876", "Onix", 2021, 1500, client.Id);

        // Act
        var result = await service.UpdateVehicleAsync(Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateVehicleAsync_WhenPlateBelongsToAnotherVehicle_ReturnsConflict()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        var vehicle = new VehicleEntity { Plate = "ABC1234", Model = "Onix", Year = 2020, Kilometers = 1000, ClientId = client.Id };
        var otherVehicle = new VehicleEntity { Plate = "XYZ9876", Model = "Gol", Year = 2019, Kilometers = 500, ClientId = client.Id };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.AddRange(vehicle, otherVehicle);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new VehicleDto("XYZ9876", "Onix", 2020, 1000, client.Id);

        // Act
        var result = await service.UpdateVehicleAsync(vehicle.Id, dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, result.Status);
    }

    [Fact]
    public async Task DeleteVehicleAsync_WhenVehicleExists_SoftDeletesVehicle()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = CreateClient();
        var vehicle = new VehicleEntity { Plate = "ABC1234", Model = "Onix", Year = 2020, Kilometers = 1000, ClientId = client.Id };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.DeleteVehicleAsync(vehicle.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var deleted = await db.Context.Vehicles.FindAsync(vehicle.Id);
        Assert.NotNull(deleted!.DeletedAt);
    }

    [Fact]
    public async Task DeleteVehicleAsync_WhenVehicleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var service = CreateService(db.Context);

        // Act
        var result = await service.DeleteVehicleAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.Status);
    }
}
