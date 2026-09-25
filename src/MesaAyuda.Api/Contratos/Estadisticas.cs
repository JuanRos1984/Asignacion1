using MesaAyuda.Api.Dominio;

namespace MesaAyuda.Api.Contratos;

public sealed record FiltroEstadisticas(DateTimeOffset? Desde, DateTimeOffset? Hasta);

public sealed record CargaTecnico(string Tecnico, int Asignados, int Resueltos);

public sealed record EstadisticasRespuesta(
    DateTimeOffset? Desde,
    DateTimeOffset? Hasta,
    int Total,
    IReadOnlyDictionary<EstadoTicket, int> PorEstado,
    IReadOnlyDictionary<Prioridad, int> PorPrioridad,
    double? PromedioHorasResolucion,
    double? PorcentajeDentroDelPlazo,
    int VencidosSinResolver,
    IReadOnlyList<CargaTecnico> PorTecnico);
