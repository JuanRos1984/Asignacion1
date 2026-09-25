using MesaAyuda.Api.Contratos;
using MesaAyuda.Api.Dominio;
using MesaAyuda.Api.Errores;
using MesaAyuda.Api.Persistencia;

namespace MesaAyuda.Api.Servicios;

/// <summary>Resúmenes agregados de la mesa de ayuda sobre los tickets creados en un rango de fechas.</summary>
public sealed class ServicioEstadisticas(IRepositorioTickets repositorio, TimeProvider reloj)
{
    public async Task<EstadisticasRespuesta> CalcularAsync(FiltroEstadisticas filtro, CancellationToken ct = default)
    {
        if (filtro.Desde > filtro.Hasta)
            throw new ValidacionException(new Dictionary<string, string[]>
            {
                ["desde"] = ["La fecha inicial no puede ser posterior a la final."]
            });

        var ahora = reloj.GetUtcNow();
        var tickets = (await repositorio.ListarAsync(ct))
            .Where(t => filtro.Desde is null || t.CreadoEn >= filtro.Desde)
            .Where(t => filtro.Hasta is null || t.CreadoEn <= filtro.Hasta)
            .ToList();

        // Se incluyen todos los valores del enum, aunque tengan cero, para que el cliente no tenga que adivinarlos.
        var porEstado = Enum.GetValues<EstadoTicket>()
            .ToDictionary(e => e, e => tickets.Count(t => t.Estado == e));
        var porPrioridad = Enum.GetValues<Prioridad>()
            .ToDictionary(p => p, p => tickets.Count(t => t.Prioridad == p));

        var resueltos = tickets.Where(t => t.ResueltoEn is not null).ToList();
        double? promedioHoras = resueltos.Count == 0
            ? null
            : Math.Round(resueltos.Average(t => (t.ResueltoEn!.Value - t.CreadoEn).TotalHours), 2);
        double? dentroDelPlazo = resueltos.Count == 0
            ? null
            : Math.Round(100.0 * resueltos.Count(t => t.ResueltoEn <= t.VenceEn) / resueltos.Count, 1);

        var porTecnico = tickets
            .Where(t => t.Tecnico is not null)
            .GroupBy(t => t.Tecnico!)
            .Select(g => new CargaTecnico(g.Key, g.Count(), g.Count(t => t.ResueltoEn is not null)))
            .OrderByDescending(c => c.Asignados)
            .ToList();

        return new EstadisticasRespuesta(
            filtro.Desde, filtro.Hasta, tickets.Count, porEstado, porPrioridad,
            promedioHoras, dentroDelPlazo, tickets.Count(t => t.EstaVencido(ahora)), porTecnico);
    }
}
