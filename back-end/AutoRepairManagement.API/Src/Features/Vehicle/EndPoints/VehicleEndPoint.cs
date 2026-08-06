using AutoRepairManagement.API.Core.Helpers;
using AutoRepairManagement.API.Features.Vehicle.DTOs;
using AutoRepairManagement.API.Features.Vehicle.Services;

namespace AutoRepairManagement.API.Features.Vehicle.EndPoints;

public static class VehicleEndPoint
{

    public static void MapVehicleEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/vehicles");
        group.MapGet("/{page:int}&{pageSize:int}", GetAllAsync);
        group.MapGet("/{vehicleId:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{vehicleId:guid}", UpdateAsync);
        group.MapDelete("/{vehicleId:guid}", DeleteAsync);
    }

    private static async Task<IResult> GetAllAsync(
        IVehicleService vehicleService,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        (await vehicleService.GetVehiclesAsync(page, pageSize, cancellationToken)).ToHttpResult();
   

    private static async Task<IResult> GetByIdAsync(
        IVehicleService vehicleService,
        Guid vehicleId,
        CancellationToken cancellationToken) => 
        (await vehicleService.GetVehicleByIdAsync(vehicleId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        IVehicleService vehicleService,
        VehicleDto vehicleDto,
        CancellationToken cancellationToken) => 
        (await vehicleService.CreateVehicleAsync(vehicleDto, cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpdateAsync(
        IVehicleService vehicleService,
        Guid vehicleId,
        VehicleDto vehicleDto,
        CancellationToken cancellationToken) => 
        (await vehicleService.UpdateVehicleAsync(vehicleId, vehicleDto, cancellationToken))
        .ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        IVehicleService vehicleService,
        Guid vehicleId,
        CancellationToken cancellationToken) => 
        (await vehicleService.DeleteVehicleAsync(vehicleId, cancellationToken))
        .ToHttpResult();
}