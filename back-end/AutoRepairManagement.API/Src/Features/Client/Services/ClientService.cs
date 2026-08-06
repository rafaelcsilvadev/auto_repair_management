using AutoRepairManagement.API.Core.Data;
using AutoRepairManagement.API.Core.Helpers;
using AutoRepairManagement.API.Features.Client.DTOs;
using AutoRepairManagement.API.Features.Client.Entities;
using AutoRepairManagement.API.Features.Client.Mappers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairManagement.API.Features.Client.Services;

public interface IClientService
{
     Task<Result> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken);
     Task<Result> GetClientsAsync(int page, int pageSize, CancellationToken cancellationToken);
     Task<Result> CreateClientAsync(ClientDto clientDto, CancellationToken cancellationToken);
     Task<Result> UpdateClientAsync(Guid clientId, ClientDto clientDto, CancellationToken cancellationToken);
     Task<Result> DeleteClientAsync(Guid clientId, CancellationToken cancellationToken);
}

public class ClientService : IClientService
{
     private readonly AppDbContext _dbContext;
     private readonly IValidator<ClientDto> _validator;
     private readonly ILogger _logger;

     public ClientService(
          AppDbContext dbContext,
          IValidator<ClientDto> validator,
          ILogger<ClientService> logger)
     {
       _logger = logger;
       _dbContext = dbContext;
       _validator = validator;
     }
  
     public async Task<Result> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken)
     {
          try
          {
               var client = await _dbContext.Clients
                    .Where(client => client.Id == clientId)
                    .Where(client => client.DeletedAt == null)
                    .Select(client => new ClientMapper(client.Id, client.Email, client.Name))
                    .FirstOrDefaultAsync(cancellationToken);
               
               return Result.Ok(data: client, page: null, pageTotal: null);
          }
          catch (Exception e)
          {
               _logger.LogError(e, "GetClientByIdAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }
     }

     public async Task<Result> GetClientsAsync(int page, int pageSize, CancellationToken cancellationToken)
     {
          try
          {
               var clients = await _dbContext.Clients
                    .Where(client => client.DeletedAt == null)
                    .OrderBy(client => client.Name)
                    .ThenBy(client => client.Email)
                    .Select(
                         client => 
                              new ClientMapper(
                                   client.Id, 
                                   client.Email, 
                                   client.Name)
                         )
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

               var totalItems = await _dbContext.Clients.CountAsync(cancellationToken);
               var pageTotal = (int)Math.Ceiling(totalItems / (double)pageSize);

               return Result.Ok(data: clients, page, pageTotal);
          } catch(Exception e)
          {
               _logger.LogError(e, "GetClientsAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }
     }

     public async Task<Result> CreateClientAsync(ClientDto clientDto, CancellationToken cancellationToken)
     {
          try
          {
               var validation = await _validator.ValidateAsync(clientDto, cancellationToken);
               if (!validation.IsValid)
               {
                    var errors = validation.Errors
                         .Select(e => e.ErrorMessage)
                         .ToList();

                    return Result.BadRequest(errors);
               };

               var isEmailUnique = await _dbContext.Clients
                    .Where(client => client.Email == clientDto.Email)
                    .Where(client => client.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);

               if(isEmailUnique != null) return Result.Conflict(["Email already exists"]);

               var clientEntity = new ClientEntity
               {
                    Name = clientDto.Name,
                    Email = clientDto.Email,
               };

               _dbContext.Add(clientEntity);
               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Created();
          }
          catch (Exception e)
          {
               _logger.LogError(e, "CreateClientAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }

     }

     public async Task<Result> UpdateClientAsync(Guid clientId, ClientDto clientDto, CancellationToken cancellationToken)
     {
          try
          {
               var validation = await _validator.ValidateAsync(clientDto, cancellationToken);
               if(!validation.IsValid) return Result.BadRequest(["Payload is invalid"]);

               var isEmailUnique = await _dbContext.Clients
                    .Where(client => client.Email == clientDto.Email)
                    .Where(client => client.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);

               if(isEmailUnique != null) return Result.Conflict(["Email already exists"]);

               var client = await _dbContext.Clients
                    .Where(client => client.Id == clientId)
                    .Where(client => client.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);
          
               if(client == null)  return  Result.NotFound(["Client not found"]);
          
               client.Name = clientDto.Name;
               client.Email = clientDto.Email;
               client.UpdatedAt = DateTime.UtcNow;

               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Ok(client, page: null, pageTotal: null);
          }
          catch (Exception e)
          {
               _logger.LogError(e, "UpdateClientAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }
         
     }

     public async Task<Result> DeleteClientAsync(Guid clientId,  CancellationToken cancellationToken)
     {
          try
          {
               var client = await _dbContext.Clients
                    .Where(client => client.Id == clientId)
                    .Where(client => client.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);

               await _dbContext.Vehicles
                    .Where(vehicle => vehicle.ClientId == clientId)
                    .Where(vehicle => vehicle.DeletedAt == null)
                    .ExecuteUpdateAsync(
                         setters => 
                              setters.SetProperty(v => v.DeletedAt, DateTime.UtcNow)
                                   .SetProperty(v => v.UpdatedAt, DateTime.UtcNow),
                         cancellationToken);

               await _dbContext.ServiceOrders
                    .Where(serviceOrder => serviceOrder.ClientId == clientId)
                    .Where(serviceOrder => serviceOrder.DeletedAt == null)
                    .ExecuteUpdateAsync(
                         setters => 
                              setters.SetProperty(v => v.DeletedAt, DateTime.UtcNow)
                                   .SetProperty(v => v.UpdatedAt, DateTime.UtcNow),
                         cancellationToken);
          
               if(client == null)  return  Result.NotFound(["Client not found"]);
          
               client.UpdatedAt = DateTime.UtcNow;
               client.DeletedAt = DateTime.UtcNow;

               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Ok(client, page: null, pageTotal: null);
          }
          catch (Exception e)
          {
               _logger.LogError(e, "DeleteClientAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }

     }
}