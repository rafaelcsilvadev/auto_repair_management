using AutoRepairManagement.API.Core.Helpers;
using AutoRepairManagement.API.Features.Client.DTOs;
using AutoRepairManagement.API.Features.Client.Services;

namespace AutoRepairManagement.API.Features.Client.EndPoints;

public static class ClientEndPoint
{

    public static void MapClientEndPoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/clients");
        group.MapGet("/{page:int}&{pageSize:int}", GetAllAsync);
        group.MapGet("/{clientId:guid}", GetByIdAsync);
        group.MapPost("/", CreateAsync);
        group.MapPut("/{clientId:guid}", UpdateAsync);
        group.MapDelete("/{clientId:guid}", DeleteAsync);
    }

    private static async Task<IResult> GetAllAsync(
        IClientService clientService,
        int page,
        int pageSize,
        CancellationToken cancellationToken) =>
        (await clientService.GetClientsAsync(page, pageSize, cancellationToken)).ToHttpResult();
   

    private static async Task<IResult> GetByIdAsync(
        IClientService clientService,
        Guid clientId,
        CancellationToken cancellationToken) => 
        (await clientService.GetClientByIdAsync(clientId, cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateAsync(
        IClientService clientService,
        ClientDto clientDto,
        CancellationToken cancellationToken) => 
        (await clientService.CreateClientAsync(clientDto, cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpdateAsync(
        IClientService clientService,
        Guid clientId,
        ClientDto clientDto,
        CancellationToken cancellationToken) => 
        (await clientService.UpdateClientAsync(clientId, clientDto, cancellationToken))
        .ToHttpResult();

    private static async Task<IResult> DeleteAsync(
        IClientService clientService,
        Guid clientId,
        CancellationToken cancellationToken) => 
        (await clientService.DeleteClientAsync(clientId, cancellationToken))
        .ToHttpResult();
  

}