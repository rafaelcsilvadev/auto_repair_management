using AutoRepairManagement.API.Core.Data;
using AutoRepairManagement.API.Core.Helpers;
using AutoRepairManagement.API.Features.Vehicle.DTOs;
using AutoRepairManagement.API.Features.Vehicle.Entities;
using AutoRepairManagement.API.Features.Vehicle.Mappers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AutoRepairManagement.API.Features.Vehicle.Services;

public interface IVehicleService
{
    Task<Result> GetVehicleByIdAsync(Guid vehicleId, CancellationToken cancellationToken);
    Task<Result> GetVehiclesAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Result> CreateVehicleAsync(VehicleDto vehicleDto, CancellationToken cancellationToken);
    Task<Result> UpdateVehicleAsync(Guid vehicleId, VehicleDto vehicleDto, CancellationToken cancellationToken);
    Task<Result> DeleteVehicleAsync(Guid vehicleId, CancellationToken cancellationToken);
}

public class VehicleService : IVehicleService
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<VehicleDto> _validator;
    private readonly ILogger _logger;

    public VehicleService(
        AppDbContext dbContext,
        IValidator<VehicleDto> validator,
        ILogger<VehicleService> logger)
    {
        _logger = logger;
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<Result> GetVehicleByIdAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await _dbContext.Vehicles
                .Where(vehicle => vehicle.Id == vehicleId)
                .Where(vehicle => vehicle.DeletedAt == null)
                .Select(vehicle => new VehicleMapper(
                    vehicle.Id, 
                    vehicle.Plate,
                    vehicle.Model,
                    vehicle.Year,
                    vehicle.Kilometers,
                    vehicle.Client.Name))
                .FirstOrDefaultAsync(cancellationToken);
               
            return Result.Ok(data: vehicle, page: null, pageTotal: null);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "GetVehicleByIdAsync");
            return Result.InternalServerError(["Internal Server Error"]);
        }
    }

    public async Task<Result> GetVehiclesAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        try
        {
            var vehicles = await _dbContext.Vehicles
                .Where(vehicle => vehicle.DeletedAt == null)
                .OrderBy(vehicle => vehicle.Plate)
                .ThenBy(vehicle => vehicle.Model)
                .Select(vehicle => new VehicleMapper(
                    vehicle.Id, 
                    vehicle.Plate,
                    vehicle.Model,
                    vehicle.Year,
                    vehicle.Kilometers,
                    vehicle.Client.Name))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var totalItems = await _dbContext.Vehicles.CountAsync(cancellationToken);
            var pageTotal = (int)Math.Ceiling(totalItems / (double)pageSize);

            return Result.Ok(data: vehicles,  page, pageTotal);
        } catch(Exception e)
        {
            _logger.LogError(e, "GetVehiclesAsync");
            return Result.InternalServerError(["Internal Server Error"]);
        }
    }

public async Task<Result> CreateVehicleAsync(VehicleDto vehicleDto, CancellationToken cancellationToken)
     {
          try
          {
               var validation = await _validator.ValidateAsync(vehicleDto, cancellationToken);
               if (!validation.IsValid)
               {
                   var errors = validation.Errors
                       .Select(e => e.ErrorMessage)
                       .ToList();

                   return Result.BadRequest(errors);
               };

               var isPlateUnique = await _dbContext.Vehicles
                   .Where(vehicle => vehicle.Plate == vehicleDto.Plate)
                   .Where(vehicle => vehicle.DeletedAt == null)
                   .FirstOrDefaultAsync(cancellationToken);

               if(isPlateUnique != null) return Result.Conflict(["Plate already exists"]);

               var vehicleEntity = new VehicleEntity
               {
                   Plate = vehicleDto.Plate,
                   Model = vehicleDto.Model,
                   Year = vehicleDto.Year,
                   Kilometers = vehicleDto.Kilometers,
                   ClientId =  vehicleDto.ClientId,
               };

                _dbContext.Add(vehicleEntity);
               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Created();
          }
          catch (Exception e)
          {
               _logger.LogError(e, "CreateVehicleAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }

     }

     public async Task<Result> UpdateVehicleAsync(Guid vehicleId, VehicleDto vehicleDto, CancellationToken cancellationToken)
     {
          try
          {
               var validation = await _validator.ValidateAsync(vehicleDto, cancellationToken);
               if (!validation.IsValid)
               {
                   var errors = validation.Errors
                       .Select(e => e.ErrorMessage)
                       .ToList();

                   return Result.BadRequest(errors);
               };

               var isPlateUnique = await _dbContext.Vehicles
                   .Where(vehicle => vehicle.Plate == vehicleDto.Plate)
                   .Where(vehicle => vehicle.DeletedAt == null)
                   .FirstOrDefaultAsync(cancellationToken);

               if(isPlateUnique != null) return Result.Conflict(["Plate already exists"]);

               var vehicle = await _dbContext.Vehicles
                    .Where(vehicle => vehicle.Id == vehicleId)
                    .Where(vehicle => vehicle.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);
          
               if(vehicle == null)  return  Result.NotFound(["Vehicle not found"]);

               vehicle.Plate = vehicleDto.Plate;
               vehicle.Model = vehicleDto.Model;
               vehicle.Year = vehicleDto.Year;
               vehicle.Kilometers = vehicleDto.Kilometers;
               vehicle.ClientId =  vehicleDto.ClientId;
               vehicle.UpdatedAt = DateTime.UtcNow;

               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Ok(vehicle, page: null, pageTotal: null);
          }
          catch (Exception e)
          {
               _logger.LogError(e, "UpdateVehicleAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }
         
     }

     public async Task<Result> DeleteVehicleAsync(Guid vehicleId,  CancellationToken cancellationToken)
     {
          try
          {
               var vehicle = await _dbContext.Vehicles
                    .Where(vehicle => vehicle.Id == vehicleId)
                    .Where(vehicle => vehicle.DeletedAt == null)
                    .FirstOrDefaultAsync(cancellationToken);
          
               if(vehicle == null) return Result.NotFound(["Vehicle already deleted"]);
          
               vehicle.UpdatedAt = DateTime.UtcNow;
               vehicle.DeletedAt = DateTime.UtcNow;

               await _dbContext.SaveChangesAsync(cancellationToken);

               return Result.Ok(vehicle, page: null, pageTotal: null);
          }
          catch (Exception e)
          {
               _logger.LogError(e, "DeleteVehicleAsync");
               return Result.InternalServerError(["Internal Server Error"]);
          }

     }
}