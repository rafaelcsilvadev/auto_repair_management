using AutoRepairManagement.API.Core.Data;
using AutoRepairManagement.API.Features.Client.DTOs;
using AutoRepairManagement.API.Features.Client.Entities;
using AutoRepairManagement.API.Features.Client.Mappers;
using AutoRepairManagement.API.Features.Client.Services;
using AutoRepairManagement.API.Features.ServiceOrder.Entities;
using AutoRepairManagement.API.Features.Vehicle.Entities;
using AutoRepairManagement.Test.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoRepairManagement.Test.Features.Client;

public class ClientServiceTests
{
    private static ClientService CreateService(AppDbContext dbContext) =>
        new(dbContext, new ClientValidator(), NullLogger<ClientService>.Instance);

    [Fact]
    public async Task GetClientByIdAsync_WhenClientExists_ReturnsOkWithClient()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = new ClientEntity { Name = "Maria Silva", Email = "maria@example.com" };
        db.Context.Clients.Add(client);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetClientByIdAsync(client.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetClientByIdAsync_WhenClientIsSoftDeleted_ReturnsOkWithNullData()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = new ClientEntity { Name = "Maria Silva", Email = "maria@example.com", DeletedAt = DateTime.UtcNow };
        db.Context.Clients.Add(client);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetClientByIdAsync(client.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetClientsAsync_ReturnsPagedResultsOrderedByName()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        db.Context.Clients.AddRange(
            new ClientEntity { Name = "Bruno", Email = "bruno@example.com" },
            new ClientEntity { Name = "Ana", Email = "ana@example.com" },
            new ClientEntity { Name = "Carlos", Email = "carlos@example.com" });
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.GetClientsAsync(page: 1, pageSize: 2, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var clients = Assert.IsType<List<ClientMapper>>(result.Data);
        Assert.Equal(2, clients.Count);
        Assert.Equal("Ana", clients[0].Name);
        Assert.Equal("ana@example.com", clients[0].Email);
        Assert.Equal("Bruno", clients[1].Name);
        Assert.Equal("bruno@example.com", clients[1].Email);
        Assert.Equal(2, result.PageTotal);
    }

    [Fact]
    public async Task CreateClientAsync_WhenPayloadIsValid_CreatesClientAndReturnsCreated()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var service = CreateService(db.Context);
        var dto = new ClientDto("Maria Silva", "maria@example.com");

        // Act
        var result = await service.CreateClientAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status201Created, result.Status);
        Assert.Single(db.Context.Clients);
    }

    [Fact]
    public async Task CreateClientAsync_WhenPayloadIsInvalid_ReturnsBadRequestAndDoesNotPersist()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var service = CreateService(db.Context);
        var dto = new ClientDto("", "not-an-email");

        // Act
        var result = await service.CreateClientAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, result.Status);
        Assert.Empty(db.Context.Clients);
    }

    [Fact]
    public async Task CreateClientAsync_WhenEmailAlreadyExists_ReturnsConflict()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        db.Context.Clients.Add(new ClientEntity { Name = "Existing", Email = "maria@example.com" });
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new ClientDto("Maria Silva", "maria@example.com");

        // Act
        var result = await service.CreateClientAsync(dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, result.Status);
    }

    [Fact]
    public async Task UpdateClientAsync_WhenClientExists_UpdatesFieldsAndReturnsOk()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = new ClientEntity { Name = "Old Name", Email = "old@example.com" };
        db.Context.Clients.Add(client);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new ClientDto("New Name", "new@example.com");

        // Act
        var result = await service.UpdateClientAsync(client.Id, dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var updated = await db.Context.Clients.FindAsync(client.Id);
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("new@example.com", updated.Email);
    }

    [Fact]
    public async Task UpdateClientAsync_WhenClientDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var service = CreateService(db.Context);
        var dto = new ClientDto("New Name", "new@example.com");

        // Act
        var result = await service.UpdateClientAsync(Guid.NewGuid(), dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.Status);
    }

    [Fact]
    public async Task UpdateClientAsync_WhenEmailBelongsToAnotherClient_ReturnsConflict()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = new ClientEntity { Name = "Client A", Email = "a@example.com" };
        var otherClient = new ClientEntity { Name = "Client B", Email = "b@example.com" };
        db.Context.Clients.AddRange(client, otherClient);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);
        var dto = new ClientDto("Client A", "b@example.com");

        // Act
        var result = await service.UpdateClientAsync(client.Id, dto, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, result.Status);
    }

    [Fact]
    public async Task DeleteClientAsync_WhenClientExists_SoftDeletesClientAndCascadesToVehiclesAndServiceOrders()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var client = new ClientEntity { Name = "Maria Silva", Email = "maria@example.com" };
        db.Context.Clients.Add(client);
        var originalCreatedAt = DateTime.UtcNow.AddDays(-30);
        var vehicle = new VehicleEntity
        {
            Plate = "ABC1234", Model = "Onix", Year = 2020, Kilometers = 1000,
            ClientId = client.Id, CreatedAt = originalCreatedAt,
        };
        db.Context.Vehicles.Add(vehicle);
        var serviceOrder = new ServiceOrderEntity
        {
            ServiceOrder = 1, Description = "Oil change", Price = 150m, EndDate = DateTime.UtcNow.AddDays(1),
            Status = "Open", ClientId = client.Id, VehicleId = vehicle.Id, CreatedAt = originalCreatedAt,
        };
        db.Context.ServiceOrders.Add(serviceOrder);
        await db.Context.SaveChangesAsync();
        var service = CreateService(db.Context);

        // Act
        var result = await service.DeleteClientAsync(client.Id, CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, result.Status);
        var deletedClient = await db.Context.Clients.FindAsync(client.Id);
        Assert.NotNull(deletedClient!.DeletedAt);
        // ExecuteUpdateAsync writes straight to the database and bypasses the change
        // tracker, so the cascade to Vehicles/ServiceOrders must be read back with AsNoTracking.
        var updatedVehicle = await db.Context.Vehicles
            .AsNoTracking()
            .FirstAsync(v => v.Id == vehicle.Id);
        Assert.NotNull(updatedVehicle.DeletedAt);
        Assert.Equal(originalCreatedAt, updatedVehicle.CreatedAt);
        Assert.NotNull(updatedVehicle.UpdatedAt);

        var updatedServiceOrder = await db.Context.ServiceOrders
            .AsNoTracking()
            .FirstAsync(o => o.Id == serviceOrder.Id);
        Assert.NotNull(updatedServiceOrder.DeletedAt);
        Assert.Equal(originalCreatedAt, updatedServiceOrder.CreatedAt);
        Assert.NotNull(updatedServiceOrder.UpdatedAt);
    }

    [Fact]
    public async Task DeleteClientAsync_WhenClientDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var db = new SqliteInMemoryAppDbContext();
        var service = CreateService(db.Context);

        // Act
        var result = await service.DeleteClientAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, result.Status);
    }
}
