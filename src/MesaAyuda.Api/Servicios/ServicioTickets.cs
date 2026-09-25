using MesaAyuda.Api.Contratos;
using MesaAyuda.Api.Dominio;
using MesaAyuda.Api.Errores;
using MesaAyuda.Api.Persistencia;

namespace MesaAyuda.Api.Servicios;

/// <summary>Casos de uso de la mesa de ayuda. Los endpoints solo traducen HTTP hacia aquí.</summary>
public sealed class ServicioTickets(IRepositorioTickets repositorio, TimeProvider reloj, ILogger<ServicioTickets> logger)
{
    public async Task<TicketRespuesta> CrearAsync(CrearTicketSolicitud solicitud, CancellationToken ct = default)
    {
        ValidadorTickets.Validar(solicitud);
        var ahora = reloj.GetUtcNow();

        var ticket = Ticket.Nuevo(
            solicitud.Titulo!.Trim(), solicitud.Descripcion!.Trim(), solicitud.Solicitante!.Trim(),
            solicitud.Prioridad!.Value, ahora);
        await repositorio.AgregarAsync(ticket, ct);

        logger.LogInformation("Ticket {Numero} creado con prioridad {Prioridad}", ticket.Numero, ticket.Prioridad);
        return TicketRespuesta.Desde(ticket, ahora);
    }

    public async Task<Pagina<TicketResumen>> ListarAsync(FiltroTickets filtro, CancellationToken ct = default)
    {
        ValidadorTickets.Validar(filtro);
        var ahora = reloj.GetUtcNow();
        var pagina = filtro.Pagina ?? 1;
        var tamano = filtro.Tamano ?? 10;

        IEnumerable<Ticket> consulta = await repositorio.ListarAsync(ct);

        if (filtro.Estado is { } estado)
            consulta = consulta.Where(t => t.Estado == estado);
        if (filtro.Prioridad is { } prioridad)
            consulta = consulta.Where(t => t.Prioridad == prioridad);
        if (!string.IsNullOrWhiteSpace(filtro.Texto))
            consulta = consulta.Where(t =>
                t.Titulo.Contains(filtro.Texto, StringComparison.OrdinalIgnoreCase) ||
                t.Descripcion.Contains(filtro.Texto, StringComparison.OrdinalIgnoreCase));
        if (filtro.Vencidos is { } vencidos)
            consulta = consulta.Where(t => t.EstaVencido(ahora) == vencidos);

        // Orden de la cola de atención: lo más urgente y lo más antiguo primero.
        var ordenados = consulta
            .OrderByDescending(t => t.Prioridad)
            .ThenBy(t => t.CreadoEn)
            .ToList();

        var elementos = ordenados
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Select(t => TicketResumen.Desde(t, ahora))
            .ToList();

        return new Pagina<TicketResumen>(elementos, pagina, tamano, ordenados.Count);
    }

    public async Task<TicketRespuesta> ObtenerAsync(int numero, CancellationToken ct = default)
    {
        var ticket = await BuscarAsync(numero, ct);
        return TicketRespuesta.Desde(ticket, reloj.GetUtcNow());
    }

    public async Task<TicketRespuesta> AsignarAsync(int numero, AsignarTecnicoSolicitud solicitud, CancellationToken ct = default)
    {
        ValidadorTickets.Validar(solicitud);
        var ticket = await BuscarAsync(numero, ct);

        if (MaquinaEstadosTicket.EsTerminal(ticket.Estado))
            throw new ValidacionException(new Dictionary<string, string[]>
            {
                ["tecnico"] = [$"No se puede asignar un ticket en estado {ticket.Estado}."]
            });

        var ahora = reloj.GetUtcNow();
        ticket.Asignar(solicitud.Tecnico!.Trim(), ahora);
        await repositorio.ActualizarAsync(ticket, ct);

        logger.LogInformation("Ticket {Numero} asignado a {Tecnico}", numero, ticket.Tecnico);
        return TicketRespuesta.Desde(ticket, ahora);
    }

    public async Task<TicketRespuesta> CambiarEstadoAsync(int numero, CambiarEstadoSolicitud solicitud, CancellationToken ct = default)
    {
        ValidadorTickets.Validar(solicitud);
        var ticket = await BuscarAsync(numero, ct);
        var anterior = ticket.Estado;

        var ahora = reloj.GetUtcNow();
        ticket.CambiarEstado(solicitud.Estado!.Value, solicitud.Nota?.Trim(), ahora);
        await repositorio.ActualizarAsync(ticket, ct);

        logger.LogInformation("Ticket {Numero}: {Anterior} -> {Nuevo}", numero, anterior, ticket.Estado);
        return TicketRespuesta.Desde(ticket, ahora);
    }

    private async Task<Ticket> BuscarAsync(int numero, CancellationToken ct) =>
        await repositorio.ObtenerAsync(numero, ct) ?? throw new TicketNoEncontradoException(numero);
}
