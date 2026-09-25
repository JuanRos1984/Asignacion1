using MesaAyuda.Api.Dominio;

namespace MesaAyuda.Api.Contratos;

public sealed record TicketRespuesta(
    int Numero,
    string Titulo,
    string Descripcion,
    string Solicitante,
    Prioridad Prioridad,
    string? Tecnico,
    EstadoTicket Estado,
    DateTimeOffset CreadoEn,
    DateTimeOffset ActualizadoEn,
    DateTimeOffset VenceEn,
    bool Vencido,
    IReadOnlyList<EstadoTicket> SiguientesEstados,
    IReadOnlyList<CambioEstado> Historial)
{
    public static TicketRespuesta Desde(Ticket t, DateTimeOffset ahora) => new(
        t.Numero, t.Titulo, t.Descripcion, t.Solicitante, t.Prioridad, t.Tecnico, t.Estado,
        t.CreadoEn, t.ActualizadoEn, t.VenceEn, t.EstaVencido(ahora),
        MaquinaEstadosTicket.SiguientesDesde(t.Estado), t.Historial);
}

public sealed record TicketResumen(
    int Numero,
    string Titulo,
    Prioridad Prioridad,
    EstadoTicket Estado,
    string? Tecnico,
    DateTimeOffset CreadoEn,
    DateTimeOffset VenceEn,
    bool Vencido)
{
    public static TicketResumen Desde(Ticket t, DateTimeOffset ahora) => new(
        t.Numero, t.Titulo, t.Prioridad, t.Estado, t.Tecnico, t.CreadoEn, t.VenceEn, t.EstaVencido(ahora));
}

public sealed record Pagina<T>(IReadOnlyList<T> Elementos, int NumeroPagina, int Tamano, int Total);
