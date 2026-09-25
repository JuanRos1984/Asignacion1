using MesaAyuda.Api.Dominio;
using MesaAyuda.Api.Persistencia;

namespace MesaAyuda.Tests;

public sealed class RepositorioTicketsJsonTests : IDisposable
{
    private static readonly DateTimeOffset Inicio = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "mesa-ayuda-pruebas", Guid.NewGuid().ToString("N"));
    private string Ruta => Path.Combine(_carpeta, "tickets.json");

    private static Ticket NuevoTicket(string titulo) =>
        Ticket.Nuevo(titulo, "Descripción de prueba del ticket.", "a@itla.edu.do", Prioridad.Alta, Inicio);

    [Fact]
    public async Task Asigna_numeros_consecutivos()
    {
        var repositorio = new RepositorioTicketsJson(Ruta);

        var primero = await repositorio.AgregarAsync(NuevoTicket("Primero"));
        var segundo = await repositorio.AgregarAsync(NuevoTicket("Segundo"));

        Assert.Equal(1, primero.Numero);
        Assert.Equal(2, segundo.Numero);
    }

    [Fact]
    public async Task Los_datos_sobreviven_a_un_reinicio()
    {
        var ticket = NuevoTicket("Persistente");
        ticket.CambiarEstado(EstadoTicket.EnProceso, "Tomado", Inicio.AddHours(1));
        await new RepositorioTicketsJson(Ruta).AgregarAsync(ticket);

        // Una instancia nueva equivale a reiniciar la aplicación.
        var leido = await new RepositorioTicketsJson(Ruta).ObtenerAsync(1);

        Assert.NotNull(leido);
        Assert.Equal("Persistente", leido.Titulo);
        Assert.Equal(EstadoTicket.EnProceso, leido.Estado);
        Assert.Single(leido.Historial);
    }

    [Fact]
    public async Task Actualizar_reemplaza_el_ticket_guardado()
    {
        var repositorio = new RepositorioTicketsJson(Ruta);
        var ticket = await repositorio.AgregarAsync(NuevoTicket("Por asignar"));

        ticket.Asignar("Soporte N2", Inicio.AddHours(1));
        await repositorio.ActualizarAsync(ticket);

        Assert.Equal("Soporte N2", (await repositorio.ObtenerAsync(ticket.Numero))!.Tecnico);
    }

    [Fact]
    public async Task Un_archivo_danado_no_se_sobrescribe()
    {
        Directory.CreateDirectory(_carpeta);
        await File.WriteAllTextAsync(Ruta, "{ esto no es json");
        var repositorio = new RepositorioTicketsJson(Ruta);

        await Assert.ThrowsAsync<InvalidOperationException>(() => repositorio.AgregarAsync(NuevoTicket("Nuevo")));

        Assert.Equal("{ esto no es json", await File.ReadAllTextAsync(Ruta));
    }

    [Fact]
    public async Task Las_altas_simultaneas_no_repiten_numero()
    {
        var repositorio = new RepositorioTicketsJson(Ruta);

        await Task.WhenAll(Enumerable.Range(1, 20).Select(i => repositorio.AgregarAsync(NuevoTicket($"Ticket {i}"))));

        var numeros = (await repositorio.ListarAsync()).Select(t => t.Numero).ToList();
        Assert.Equal(Enumerable.Range(1, 20), numeros.Order());
    }

    public void Dispose()
    {
        if (Directory.Exists(_carpeta))
            Directory.Delete(_carpeta, recursive: true);
    }
}
