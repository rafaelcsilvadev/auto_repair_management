using AutoRepairManagement.API.Core.Data;
using AutoRepairManagement.API.Features.Client.Entities;
using AutoRepairManagement.API.Features.ServiceOrder.DTOs;
using AutoRepairManagement.API.Features.ServiceOrder.Entities;
using AutoRepairManagement.API.Features.ServiceOrder.Mappers;
using AutoRepairManagement.API.Features.ServiceOrder.Services;
using AutoRepairManagement.API.Features.Vehicle.Entities;
using AutoRepairManagement.Test.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoRepairManagement.Test.Features.ServiceOrder;

public class ServiceOrderServiceTests
{
    private static ServiceOrderService CreateService(AppDbContext dbContext) =>
        new(dbContext, new ServiceOrderValidator(), NullLogger<ServiceOrderService>.Instance);

    private static (ClientEntity Client, VehicleEntity Vehicle) CreateClientAndVehicle(
        string email = "maria@example.com", string plate = "ABC1234")
    {
        var client = new ClientEntity { Name = "Maria Silva", Email = email };
        var vehicle = new VehicleEntity { Plate = plate, Model = "Onix", Year = 2020, Kilometers = 1000, ClientId = client.Id };
        return (client, vehicle);
    }

    [Fact]
    public async Task GetServiceOrderByIdAsync_WhenOrderExists_ReturnsOkWithClientAndVehicleData()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var (client, vehicle) = CreateClientAndVehicle();
        var order = new ServiceOrderEntity
        {
            ServiceOrder = 1, Description = "Oil change", Price = 150m, EndDate = DateTime.UtcNow.AddDays(1),
            Status = "Open", ClientId = client.Id, VehicleId = vehicle.Id,
        };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        db.Context.ServiceOrders.Add(order);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetServiceOrderByIdAsync(order.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var mapped = Assert.IsType<ServiceOrderMapper>(result.Data);
        Assert.Equal("Maria Silva", mapped.ClientName);
        Assert.Equal("ABC1234", mapped.Plate);
    }

    [Fact]
    public async Task GetServiceOrderByIdAsync_WhenOrderIsSoftDeleted_ReturnsOkWithNullData()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var (client, vehicle) = CreateClientAndVehicle();
        var order = new ServiceOrderEntity
        {
            ServiceOrder = 1, Description = "Oil change", Price = 150m, EndDate = DateTime.UtcNow.AddDays(1),
            Status = "Open", ClientId = client.Id, VehicleId = vehicle.Id, DeletedAt = DateTime.UtcNow,
        };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        db.Context.ServiceOrders.Add(order);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetServiceOrderByIdAsync(order.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetServiceOrdersAsync_ReturnsPagedResultsOrderedByNumber()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        // Each order needs its own Client/Vehicle: ClientEntity.ServiceOrder and
        // VehicleEntity.ServiceOrder are singular nav properties, so EF Core's convention
        // configures ClientId/VehicleId on ServiceOrders as UNIQUE foreign keys — a client
        // or vehicle can only ever have one ServiceOrder (see CreateServiceOrderAsync_*
        // tests below for the resulting data-loss bug).
        var (clientA, vehicleA) = CreateClientAndVehicle(email: "a@example.com", plate: "AAA1111");
        var (clientB, vehicleB) = CreateClientAndVehicle(email: "b@example.com", plate: "BBB2222");
        var (clientC, vehicleC) = CreateClientAndVehicle(email: "c@example.com", plate: "CCC3333");
        db.Context.Clients.AddRange(clientA, clientB, clientC);
        db.Context.Vehicles.AddRange(vehicleA, vehicleB, vehicleC);
        db.Context.ServiceOrders.AddRange(
            new ServiceOrderEntity { ServiceOrder = 2, Description = "Brake pads", Price = 200m, EndDate = DateTime.UtcNow, Status = "Open", ClientId = clientB.Id, VehicleId = vehicleB.Id },
            new ServiceOrderEntity { ServiceOrder = 1, Description = "Oil change", Price = 150m, EndDate = DateTime.UtcNow, Status = "Open", ClientId = clientA.Id, VehicleId = vehicleA.Id },
            new ServiceOrderEntity { ServiceOrder = 3, Description = "Tire rotation", Price = 80m, EndDate = DateTime.UtcNow, Status = "Open", ClientId = clientC.Id, VehicleId = vehicleC.Id });
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetServiceOrdersAsync(page: 1, pageSize: 2, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var orders = Assert.IsType<List<ServiceOrderMapper>>(result.Data);
        Assert.Equal(2, orders.Count);
        Assert.Equal("Oil change", orders[0].Description);
        Assert.Equal("Brake pads", orders[1].Description);
        Assert.Equal(2, result.PageTotal);
    }

    [Fact]
    public async Task CreateServiceOrderAsync_WhenNoOrdersExist_AssignsNumberOne()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var (client, vehicle) = CreateClientAndVehicle();
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new ServiceOrderDto("Oil change", 150m, "Open", DateTime.UtcNow.AddDays(1), client.Id, vehicle.Id);

        // Act
        var result = await service.CreateServiceOrderAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, result.Status);
        var created = await db.Context.ServiceOrders.SingleAsync();
        Assert.Equal(1, created.ServiceOrder);
    }

    [Fact]
    public async Task CreateServiceOrderAsync_WhenOrdersAlreadyExist_AssignsNextNumber()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var (existingClient, existingVehicle) = CreateClientAndVehicle(email: "a@example.com", plate: "AAA1111");
        var (newClient, newVehicle) = CreateClientAndVehicle(email: "b@example.com", plate: "BBB2222");
        db.Context.Clients.AddRange(existingClient, newClient);
        db.Context.Vehicles.AddRange(existingVehicle, newVehicle);
        db.Context.ServiceOrders.Add(new ServiceOrderEntity
        {
            ServiceOrder = 1, Description = "Oil change", Price = 150m, EndDate = DateTime.UtcNow,
            Status = "Open", ClientId = existingClient.Id, VehicleId = existingVehicle.Id,
        });
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new ServiceOrderDto("Brake pads", 200m, "Open", DateTime.UtcNow.AddDays(2), newClient.Id, newVehicle.Id);

        // Act
        var result = await service.CreateServiceOrderAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, result.Status);
        var created = await db.Context.ServiceOrders.SingleAsync(o => o.Description == "Brake pads");
        Assert.Equal(2, created.ServiceOrder);
    }

    [Fact]
    public async Task CreateServiceOrderAsync_WhenClientAndVehicleAlreadyHaveAnOrder_CreatesBothOrders()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var (client, vehicle) = CreateClientAndVehicle();
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var firstResult = await service.CreateServiceOrderAsync(
            new ServiceOrderDto("Oil change", 150m, "Open", DateTime.UtcNow.AddDays(1), client.Id, vehicle.Id),
            CancellationToken.None);
        var secondResult = await service.CreateServiceOrderAsync(
            new ServiceOrderDto("Brake pads", 200m, "Open", DateTime.UtcNow.AddDays(2), client.Id, vehicle.Id),
            CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, firstResult.Status);
        Assert.Equal(StatusCodes.Status201Created, secondResult.Status);
        var orders = await db.Context.ServiceOrders.ToListAsync();
        Assert.Equal(2, orders.Count);
        Assert.Contains(orders, o => o.Description == "Oil change");
        Assert.Contains(orders, o => o.Description == "Brake pads");
    }

    [Fact]
    public async Task CreateServiceOrderAsync_WhenPayloadIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var service = CreateService(db.Context);
        var dto = new ServiceOrderDto("", 0m, "", DateTime.UtcNow, Guid.Empty, Guid.Empty);

        // Act
        var result = await service.CreateServiceOrderAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, result.Status);
        Assert.Empty(db.Context.ServiceOrders);
    }

    [Fact]
    public async Task UpdateServiceOrderAsync_WhenOrderExists_UpdatesFieldsAndReturnsOk()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var (client, vehicle) = CreateClientAndVehicle();
        var order = new ServiceOrderEntity
        {
            ServiceOrder = 1, Description = "Oil change", Price = 150m, EndDate = DateTime.UtcNow,
            Status = "Open", ClientId = client.Id, VehicleId = vehicle.Id,
        };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        db.Context.ServiceOrders.Add(order);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new ServiceOrderDto("Oil change (full synthetic)", 220m, "Closed", DateTime.UtcNow.AddDays(1), client.Id, vehicle.Id);

        // Act
        var result = await service.UpdateServiceOrderAsync(order.Id, dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var updated = await db.Context.ServiceOrders.FindAsync(order.Id);
        Assert.Equal("Oil change (full synthetic)", updated!.Description);
        Assert.Equal("Closed", updated.Status);
        Assert.Equal(220m, updated.Price);
    }

    [Fact]
    public async Task UpdateServiceOrderAsync_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var (client, vehicle) = CreateClientAndVehicle();
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new ServiceOrderDto("Oil change", 150m, "Open", DateTime.UtcNow.AddDays(1), client.Id, vehicle.Id);

        // Act
        var result = await service.UpdateServiceOrderAsync(Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.Status);
    }

    [Fact]
    public async Task DeleteServiceOrderAsync_WhenOrderExists_SoftDeletesOrder()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var (client, vehicle) = CreateClientAndVehicle();
        var order = new ServiceOrderEntity
        {
            ServiceOrder = 1, Description = "Oil change", Price = 150m, EndDate = DateTime.UtcNow,
            Status = "Open", ClientId = client.Id, VehicleId = vehicle.Id,
        };
        db.Context.Clients.Add(client);
        db.Context.Vehicles.Add(vehicle);
        db.Context.ServiceOrders.Add(order);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.DeleteServiceOrderAsync(order.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var deleted = await db.Context.ServiceOrders.FindAsync(order.Id);
        Assert.NotNull(deleted!.DeletedAt);
    }

    [Fact]
    public async Task DeleteServiceOrderAsync_WhenOrderDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var service = CreateService(db.Context);

        // Act
        var result = await service.DeleteServiceOrderAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.Status);
    }
}
