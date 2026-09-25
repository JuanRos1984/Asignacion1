using System.Text.Json.Serialization;

namespace MesaAyuda.Api.Dominio;

public sealed class Ticket
{
    public int Numero { get; set; }
    public required string Titulo { get; init; }
    public required string Descripcion { get; init; }
    public required string Solicitante { get; init; }
    public Prioridad Prioridad { get; init; }
    [JsonInclude]
    public string? Tecnico { get; private set; }

    [JsonInclude]
    public EstadoTicket Estado { get; private set; } = EstadoTicket.Abierto;

    public DateTimeOffset CreadoEn { get; init; }

    [JsonInclude]
    public DateTimeOffset ActualizadoEn { get; private set; }

    [JsonInclude]
    public DateTimeOffset? ResueltoEn { get; private set; }

    [JsonInclude]
    public List<CambioEstado> Historial { get; private set; } = [];

    /// <summary>Hora límite de atención según la prioridad (acuerdo de nivel de servicio).</summary>
    public DateTimeOffset VenceEn => CreadoEn + PlazoSegun(Prioridad);

    public bool EstaVencido(DateTimeOffset ahora) =>
        ResueltoEn is null && !MaquinaEstadosTicket.EsTerminal(Estado) && ahora > VenceEn;

    public void CambiarEstado(EstadoTicket nuevo, string? nota, DateTimeOffset ahora)
    {
        if (!MaquinaEstadosTicket.PuedeTransicionar(Estado, nuevo))
            throw new TransicionInvalidaException(Estado, nuevo);

        Historial.Add(new CambioEstado(Estado, nuevo, ahora, nota));
        Estado = nuevo;
        ActualizadoEn = ahora;

        if (nuevo == EstadoTicket.Resuelto)
            ResueltoEn = ahora;
        else if (nuevo == EstadoTicket.EnProceso)
            ResueltoEn = null; // reabierto: la resolución anterior deja de contar
    }

    public void Asignar(string tecnico, DateTimeOffset ahora)
    {
        Tecnico = tecnico;
        ActualizadoEn = ahora;
    }

    public static TimeSpan PlazoSegun(Prioridad prioridad) => prioridad switch
    {
        Prioridad.Critica => TimeSpan.FromHours(4),
        Prioridad.Alta => TimeSpan.FromHours(8),
        Prioridad.Media => TimeSpan.FromHours(24),
        _ => TimeSpan.FromHours(72)
    };

    public static Ticket Nuevo(string titulo, string descripcion, string solicitante, Prioridad prioridad, DateTimeOffset ahora) =>
        new()
        {
            Titulo = titulo,
            Descripcion = descripcion,
            Solicitante = solicitante,
            Prioridad = prioridad,
            CreadoEn = ahora,
            ActualizadoEn = ahora
        };
}
