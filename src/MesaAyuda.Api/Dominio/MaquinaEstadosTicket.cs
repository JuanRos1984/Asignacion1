namespace MesaAyuda.Api.Dominio;

/// <summary>
/// Único punto del sistema donde se decide qué transiciones de estado son válidas.
/// Agregar una transición nueva se hace solo en esta tabla.
/// </summary>
public static class MaquinaEstadosTicket
{
    private static readonly Dictionary<EstadoTicket, EstadoTicket[]> Permitidas = new()
    {
        [EstadoTicket.Abierto] = [EstadoTicket.EnProceso, EstadoTicket.Cancelado],
        [EstadoTicket.EnProceso] = [EstadoTicket.Resuelto, EstadoTicket.Cancelado],
        // Si el solicitante no queda conforme, el ticket resuelto vuelve a trabajarse.
        [EstadoTicket.Resuelto] = [EstadoTicket.Cerrado, EstadoTicket.EnProceso],
        [EstadoTicket.Cerrado] = [],
        [EstadoTicket.Cancelado] = []
    };

    // Estas transiciones exigen dejar escrito el motivo.
    private static readonly HashSet<EstadoTicket> ExigenNota = [EstadoTicket.Resuelto, EstadoTicket.Cancelado];

    public static bool PuedeTransicionar(EstadoTicket desde, EstadoTicket hacia) =>
        Permitidas[desde].Contains(hacia);

    public static IReadOnlyList<EstadoTicket> SiguientesDesde(EstadoTicket estado) => Permitidas[estado];

    public static bool EsTerminal(EstadoTicket estado) => Permitidas[estado].Length == 0;

    public static bool ExigeNota(EstadoTicket hacia) => ExigenNota.Contains(hacia);
}
