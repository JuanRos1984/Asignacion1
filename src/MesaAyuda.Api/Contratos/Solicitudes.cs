using MesaAyuda.Api.Dominio;

namespace MesaAyuda.Api.Contratos;

// Los campos son anulables a propósito: la ausencia de un dato se reporta como error
// de validación con su nombre, no como un fallo genérico de lectura del cuerpo.

public sealed record CrearTicketSolicitud(string? Titulo, string? Descripcion, string? Solicitante, Prioridad? Prioridad);

public sealed record AsignarTecnicoSolicitud(string? Tecnico);

public sealed record CambiarEstadoSolicitud(EstadoTicket? Estado, string? Nota);

public sealed record FiltroTickets(
    EstadoTicket? Estado,
    Prioridad? Prioridad,
    string? Texto,
    bool? Vencidos,
    int? Pagina,
    int? Tamano);
