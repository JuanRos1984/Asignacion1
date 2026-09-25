using MesaAyuda.Api.Contratos;
using MesaAyuda.Api.Servicios;

namespace MesaAyuda.Api.Endpoints;

public static class TicketsEndpoints
{
    public static IEndpointRouteBuilder MapTickets(this IEndpointRouteBuilder rutas)
    {
        var grupo = rutas.MapGroup("/api/tickets").WithTags("Tickets");

        grupo.MapPost("/", async (CrearTicketSolicitud solicitud, ServicioTickets servicio, CancellationToken ct) =>
        {
            var creado = await servicio.CrearAsync(solicitud, ct);
            return Results.Created($"/api/tickets/{creado.Numero}", creado);
        });

        grupo.MapGet("/", async ([AsParameters] FiltroTickets filtro, ServicioTickets servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ListarAsync(filtro, ct)));

        grupo.MapGet("/{numero:int}", async (int numero, ServicioTickets servicio, CancellationToken ct) =>
            Results.Ok(await servicio.ObtenerAsync(numero, ct)));

        grupo.MapPut("/{numero:int}/tecnico", async (int numero, AsignarTecnicoSolicitud solicitud, ServicioTickets servicio, CancellationToken ct) =>
            Results.Ok(await servicio.AsignarAsync(numero, solicitud, ct)));

        grupo.MapPost("/{numero:int}/estado", async (int numero, CambiarEstadoSolicitud solicitud, ServicioTickets servicio, CancellationToken ct) =>
            Results.Ok(await servicio.CambiarEstadoAsync(numero, solicitud, ct)));

        return rutas;
    }
}
