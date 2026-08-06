using AutoRepairManagement.API.Core.Helpers;
using AutoRepairManagement.API.Features.ServiceOrder.DTOs;
using AutoRepairManagement.API.Features.ServiceOrder.Services;

namespace AutoRepairManagement.API.Features.ServiceOrder.EndPoints;

public static class ServiceOrderEndPoint
{

    public static void MapServiceOrderEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/serviceOrders");
        group.MapGet("/{page:int}&{pageSize:int}", GetAllAsync);
        group.MapGet("/{serviceOrderId:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{serviceOrderId:guid}", UpdateAsync);
        group.MapDelete("/{serviceOrderId:guid}", DeleteAsync);
    }

    private static async Task<IResult> GetAllAsync(
        IServiceOrderService serviceOrderService,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        (await serviceOrderService.GetServiceOrdersAsync(page, pageSize, cancellationToken)).ToHttpResult();
   

    private static async Task<IResult> GetByIdAsync(
        IServiceOrderService serviceOrderService,
        Guid serviceOrderId,
        CancellationToken cancellationToken) => 
        (await serviceOrderService.GetServiceOrderByIdAsync(serviceOrderId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        IServiceOrderService serviceOrderService,
        ServiceOrderDto serviceOrderDto,
        CancellationToken cancellationToken) => 
        (await serviceOrderService.CreateServiceOrderAsync(serviceOrderDto, cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpdateAsync(
        IServiceOrderService serviceOrderService,
        Guid serviceOrderId,
        ServiceOrderDto serviceOrderDto,
        CancellationToken cancellationToken) => 
        (await serviceOrderService.UpdateServiceOrderAsync(serviceOrderId, serviceOrderDto, cancellationToken))
        .ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        IServiceOrderService serviceOrderService,
        Guid serviceOrderId,
        CancellationToken cancellationToken) => 
        (await serviceOrderService.DeleteServiceOrderAsync(serviceOrderId, cancellationToken))
        .ToHttpResult();
  

}