using MesaAyuda.Api.Contratos;
using MesaAyuda.Api.Dominio;
using MesaAyuda.Api.Errores;
using MesaAyuda.Api.Persistencia;
using MesaAyuda.Api.Servicios;
using Microsoft.Extensions.Logging.Abstractions;

namespace MesaAyuda.Tests;

public sealed class ServiciosTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "mesa-ayuda-pruebas", Guid.NewGuid().ToString("N"));
    private readonly RelojFijo _reloj = new(new DateTimeOffset(2026, 9, 25, 12, 0, 0, TimeSpan.Zero));
    private readonly RepositorioTicketsJson _repositorio;
    private readonly ServicioTickets _tickets;
    private readonly ServicioEstadisticas _estadisticas;

    public ServiciosTests()
    {
        _repositorio = new RepositorioTicketsJson(Path.Combine(_carpeta, "tickets.json"));
        _tickets = new ServicioTickets(_repositorio, _reloj, NullLogger<ServicioTickets>.Instance);
        _estadisticas = new ServicioEstadisticas(_repositorio, _reloj);
    }

    private Task<TicketRespuesta> CrearAsync(Prioridad prioridad = Prioridad.Media, string titulo = "Proyector sin imagen") =>
        _tickets.CrearAsync(new CrearTicketSolicitud(titulo, "El proyector del aula no muestra imagen.", "docente@itla.edu.do", prioridad));

    [Fact]
    public async Task Crear_con_datos_invalidos_reporta_cada_campo()
    {
        var error = await Assert.ThrowsAsync<ValidacionException>(() =>
            _tickets.CrearAsync(new CrearTicketSolicitud("", null, "no-es-correo", null)));

        Assert.Equal(["descripcion", "prioridad", "solicitante", "titulo"], error.Errores.Keys.Order());
        Assert.Empty(await _repositorio.ListarAsync());
    }

    [Fact]
    public async Task Resolver_sin_nota_se_rechaza_y_el_estado_no_cambia()
    {
        var creado = await CrearAsync();
        await _tickets.CambiarEstadoAsync(creado.Numero, new CambiarEstadoSolicitud(EstadoTicket.EnProceso, null));

        await Assert.ThrowsAsync<ValidacionException>(() =>
            _tickets.CambiarEstadoAsync(creado.Numero, new CambiarEstadoSolicitud(EstadoTicket.Resuelto, "  ")));

        Assert.Equal(EstadoTicket.EnProceso, (await _tickets.ObtenerAsync(creado.Numero)).Estado);
    }

    [Fact]
    public async Task No_se_asigna_tecnico_a_un_ticket_cancelado()
    {
        var creado = await CrearAsync();
        await _tickets.CambiarEstadoAsync(creado.Numero, new CambiarEstadoSolicitud(EstadoTicket.Cancelado, "Duplicado"));

        await Assert.ThrowsAsync<ValidacionException>(() =>
            _tickets.AsignarAsync(creado.Numero, new AsignarTecnicoSolicitud("Soporte N1")));
    }

    [Fact]
    public async Task Un_ticket_inexistente_se_reporta_como_no_encontrado()
    {
        await Assert.ThrowsAsync<TicketNoEncontradoException>(() => _tickets.ObtenerAsync(42));
    }

    [Fact]
    public async Task La_cola_pone_primero_lo_mas_urgente_y_filtra_vencidos()
    {
        await CrearAsync(Prioridad.Baja, "Cambiar mouse");
        await CrearAsync(Prioridad.Critica, "Servidor caído");
        _reloj.Avanzar(TimeSpan.FromHours(5)); // vence el crítico (4 h), el de baja no (72 h)

        var cola = await _tickets.ListarAsync(new FiltroTickets(null, null, null, null, null, null));
        var vencidos = await _tickets.ListarAsync(new FiltroTickets(null, null, null, true, null, null));

        Assert.Equal(["Servidor caído", "Cambiar mouse"], cola.Elementos.Select(t => t.Titulo));
        Assert.Equal("Servidor caído", Assert.Single(vencidos.Elementos).Titulo);
    }

    [Fact]
    public async Task Las_estadisticas_agrupan_y_promedian()
    {
        var a = await CrearAsync(Prioridad.Alta, "Ticket A");
        var b = await CrearAsync(Prioridad.Alta, "Ticket B");
        await CrearAsync(Prioridad.Baja, "Ticket C");

        foreach (var (numero, horas) in new[] { (a.Numero, 2), (b.Numero, 10) })
        {
            await _tickets.CambiarEstadoAsync(numero, new CambiarEstadoSolicitud(EstadoTicket.EnProceso, null));
            _reloj.Avanzar(TimeSpan.FromHours(horas));
            await _tickets.CambiarEstadoAsync(numero, new CambiarEstadoSolicitud(EstadoTicket.Resuelto, "Listo"));
        }

        var resumen = await _estadisticas.CalcularAsync(new FiltroEstadisticas(null, null));

        Assert.Equal(3, resumen.Total);
        Assert.Equal(2, resumen.PorEstado[EstadoTicket.Resuelto]);
        Assert.Equal(2, resumen.PorPrioridad[Prioridad.Alta]);
        Assert.Equal(7, resumen.PromedioHorasResolucion); // A a las 2 h, B a las 12 h
        Assert.Equal(50, resumen.PorcentajeDentroDelPlazo); // plazo de Alta: 8 h
    }

    [Fact]
    public async Task Las_estadisticas_rechazan_un_rango_invertido()
    {
        var hoy = _reloj.GetUtcNow();

        await Assert.ThrowsAsync<ValidacionException>(() =>
            _estadisticas.CalcularAsync(new FiltroEstadisticas(hoy, hoy.AddDays(-1))));
    }

    public void Dispose()
    {
        if (Directory.Exists(_carpeta))
            Directory.Delete(_carpeta, recursive: true);
    }
}
