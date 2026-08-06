using AutoRepairManagement.API.Core.Data;
using AutoRepairManagement.API.Core.Helpers;
using AutoRepairManagement.API.Features.ServiceOrder.DTOs;
using AutoRepairManagement.API.Features.ServiceOrder.Entities;
using AutoRepairManagement.API.Features.ServiceOrder.Mappers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairManagement.API.Features.ServiceOrder.Services;

public interface IServiceOrderService
{
    Task<Result> GetServiceOrderByIdAsync(Guid serviceOrderId, CancellationToken cancellationToken);
    Task<Result> GetServiceOrdersAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Result> CreateServiceOrderAsync(ServiceOrderDto serviceOrderDto, CancellationToken cancellationToken);
    Task<Result> UpdateServiceOrderAsync(Guid serviceOrderId, ServiceOrderDto serviceOrderDto, CancellationToken cancellationToken);
    Task<Result> DeleteServiceOrderAsync(Guid serviceOrderId, CancellationToken cancellationToken);
}

public class ServiceOrderService : IServiceOrderService
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<ServiceOrderDto> _validator;
    private readonly ILogger _logger;

    public ServiceOrderService(
        AppDbContext dbContext,
        IValidator<ServiceOrderDto> validator,
        ILogger<ServiceOrderService> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<Result> GetServiceOrderByIdAsync(Guid serviceOrderId, CancellationToken cancellationToken)
    {
        try
        {
            var serviceOrder = await _dbContext.ServiceOrders
                .Where(serviceOrder => serviceOrder.Id == serviceOrderId)
                .Where(serviceOrder => serviceOrder.DeletedAt == null)
                .Select(serviceOrder => new ServiceOrderMapper(
                    serviceOrder.Id,
                    serviceOrder.Client.Name,
                    serviceOrder.Vehicle.Plate,
                    serviceOrder.Description,
                    serviceOrder.Price,
                    serviceOrder.StartDate,
                    serviceOrder.EndDate,
                    serviceOrder.Status))
                .FirstOrDefaultAsync(cancellationToken);
               
            return Result.Ok(data: serviceOrder, page: null, pageTotal: null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetServiceOrderByIdAsync");
            return Result.InternalServerError(["Internal Server Error"]);
        }
    }

    public async Task<Result> GetServiceOrdersAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        try
        {
            var serviceOrders = await _dbContext.ServiceOrders
                .Where(serviceOrder => serviceOrder.DeletedAt == null)
                .OrderBy(serviceOrder => serviceOrder.ServiceOrder)
                .ThenBy(serviceOrder => serviceOrder.Vehicle.Plate)
                .Select(serviceOrder => new ServiceOrderMapper(
                    serviceOrder.Id,
                    serviceOrder.Client.Name,
                    serviceOrder.Vehicle.Plate,
                    serviceOrder.Description,
                    serviceOrder. Price,
                    serviceOrder.StartDate,
                    serviceOrder.EndDate,
                    serviceOrder.Status))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var totalItems = await _dbContext.ServiceOrders.CountAsync(cancellationToken);
            var pageTotal = (int)Math.Ceiling(totalItems / (double)pageSize);

            return Result.Ok(data: serviceOrders, page, pageTotal);
        } catch(Exception e)
        {
            _logger.LogError(e, "GetServiceOrdersAsync");
            return Result.InternalServerError(["Internal Server Error"]);
        }
    }

public async Task<Result> CreateServiceOrderAsync(ServiceOrderDto serviceOrderDto, CancellationToken cancellationToken)
     {
          try
          {
               var validation = await _validator.ValidateAsync(serviceOrderDto, cancellationToken);
               if (!validation.IsValid)
               {
                   var errors = validation.Errors
                       .Select(e => e.ErrorMessage)
                       .ToList();

                   return Result.BadRequest(errors);
               };

               var lastServiceOrder = await _dbContext.ServiceOrders
                   .OrderByDescending(e => e.CreatedAt)
                   .Select(e => e.ServiceOrder)
                   .FirstOrDefaultAsync(cancellationToken);

               var serviceOrderEntity = new ServiceOrderEntity
               {
                   ServiceOrder = lastServiceOrder + 1, 
                   Description = serviceOrderDto.Description, 
                   Price = serviceOrderDto.Price,
                   EndDate = serviceOrderDto.EndDate,
                   Status = serviceOrderDto.Status,
                   VehicleId = serviceOrderDto.VehicleId,
                   ClientId = serviceOrderDto.ClientId,
               };

                _dbContext.Add(serviceOrderEntity);
               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Created();
          }
          catch (Exception e)
          {
               _logger.LogError(e, "CreateServiceOrderAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }

     }

     public async Task<Result> UpdateServiceOrderAsync(Guid serviceOrderId, ServiceOrderDto serviceOrderDto, CancellationToken cancellationToken)
     {
          try
          {
               var validation = await _validator.ValidateAsync(serviceOrderDto, cancellationToken);
               if (!validation.IsValid)
               {
                   var errors = validation.Errors
                       .Select(e => e.ErrorMessage)
                       .ToList();

                   return Result.BadRequest(errors);
               };

               var serviceOrder = await _dbContext.ServiceOrders
                    .Where(serviceOrder => serviceOrder.Id == serviceOrderId)
                    .Where(serviceOrder => serviceOrder.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);
          
               if(serviceOrder == null)  return  Result.NotFound(["ServiceOrder not found"]);

               serviceOrder.Description = serviceOrderDto.Description;
               serviceOrder.Price = serviceOrderDto.Price;
               serviceOrder.EndDate = serviceOrderDto.EndDate;
               serviceOrder.Status = serviceOrderDto.Status;
               serviceOrder.VehicleId = serviceOrderDto.VehicleId;
               serviceOrder.ClientId = serviceOrderDto.ClientId;
               serviceOrder.UpdatedAt = DateTime.UtcNow;

               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Ok(serviceOrder, page: null, pageTotal: null);
          }
          catch (Exception e)
          {
               _logger.LogError(e, "UpdateServiceOrderAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }
         
     }

     public async Task<Result> DeleteServiceOrderAsync(Guid serviceOrderId,  CancellationToken cancellationToken)
     {
          try
          {
               var serviceOrder = await _dbContext.ServiceOrders
                    .Where(serviceOrder => serviceOrder.Id == serviceOrderId)
                    .Where(serviceOrder => serviceOrder.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);
          
               if(serviceOrder == null) return Result.NotFound(["ServiceOrder already deleted"]);
          
               serviceOrder.UpdatedAt = DateTime.UtcNow;
               serviceOrder.DeletedAt = DateTime.UtcNow;

               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Ok(serviceOrder, page: null, pageTotal: null);
          }
          catch (Exception e)
          {
               _logger.LogError(e, "DeleteServiceOrderAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }

     }
}