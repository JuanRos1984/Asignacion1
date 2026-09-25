using MesaAyuda.Api.Contratos;
using MesaAyuda.Api.Servicios;

namespace MesaAyuda.Api.Endpoints;

public static class EstadisticasEndpoints
{
    public static IEndpointRouteBuilder MapEstadisticas(this IEndpointRouteBuilder rutas)
    {
        rutas.MapGet("/api/estadisticas", async ([AsParameters] FiltroEstadisticas filtro, ServicioEstadisticas servicio, CancellationToken ct) =>
                Results.Ok(await servicio.CalcularAsync(filtro, ct)))
            .WithTags("Estadísticas");

        return rutas;
    }
}
