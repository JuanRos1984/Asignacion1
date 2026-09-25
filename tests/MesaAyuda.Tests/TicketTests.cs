using MesaAyuda.Api.Dominio;

namespace MesaAyuda.Tests;

public class TicketTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);

    private static Ticket NuevoTicket(Prioridad prioridad = Prioridad.Media) =>
        Ticket.Nuevo("Sin red en el aula", "El aula 204 no tiene conexión a la red.", "docente@itla.edu.do", prioridad, Inicio);

    [Fact]
    public void Nace_abierto_y_sin_historial()
    {
        var ticket = NuevoTicket();

        Assert.Equal(EstadoTicket.Abierto, ticket.Estado);
        Assert.Empty(ticket.Historial);
    }

    [Fact]
    public void Una_transicion_prohibida_lanza_y_no_cambia_el_estado()
    {
        var ticket = NuevoTicket();

        var error = Assert.Throws<TransicionInvalidaException>(() =>
            ticket.CambiarEstado(EstadoTicket.Resuelto, "sin atender", Inicio.AddHours(1)));

        Assert.Equal(EstadoTicket.Abierto, error.Desde);
        Assert.Equal(EstadoTicket.Abierto, ticket.Estado);
        Assert.Empty(ticket.Historial);
    }

    [Fact]
    public void Cada_cambio_queda_en_el_historial()
    {
        var ticket = NuevoTicket();

        ticket.CambiarEstado(EstadoTicket.EnProceso, null, Inicio.AddHours(1));
        ticket.CambiarEstado(EstadoTicket.Resuelto, "Se reinició el switch.", Inicio.AddHours(2));

        Assert.Collection(ticket.Historial,
            c => Assert.Equal((EstadoTicket.Abierto, EstadoTicket.EnProceso), (c.Desde, c.Hacia)),
            c => Assert.Equal("Se reinició el switch.", c.Nota));
        Assert.Equal(Inicio.AddHours(2), ticket.ResueltoEn);
    }

    [Fact]
    public void Reabrir_un_ticket_resuelto_borra_la_fecha_de_resolucion()
    {
        var ticket = NuevoTicket();
        ticket.CambiarEstado(EstadoTicket.EnProceso, null, Inicio.AddHours(1));
        ticket.CambiarEstado(EstadoTicket.Resuelto, "Listo", Inicio.AddHours(2));

        ticket.CambiarEstado(EstadoTicket.EnProceso, "Volvió a fallar", Inicio.AddHours(3));

        Assert.Null(ticket.ResueltoEn);
    }

    [Theory]
    [InlineData(Prioridad.Critica, 4)]
    [InlineData(Prioridad.Alta, 8)]
    [InlineData(Prioridad.Media, 24)]
    [InlineData(Prioridad.Baja, 72)]
    public void El_plazo_depende_de_la_prioridad(Prioridad prioridad, int horas)
    {
        var ticket = NuevoTicket(prioridad);

        Assert.Equal(Inicio.AddHours(horas), ticket.VenceEn);
        Assert.False(ticket.EstaVencido(Inicio.AddHours(horas)));
        Assert.True(ticket.EstaVencido(Inicio.AddHours(horas).AddMinutes(1)));
    }

    [Fact]
    public void Un_ticket_resuelto_no_se_marca_vencido()
    {
        var ticket = NuevoTicket(Prioridad.Critica);
        ticket.CambiarEstado(EstadoTicket.EnProceso, null, Inicio.AddHours(1));
        ticket.CambiarEstado(EstadoTicket.Resuelto, "Listo", Inicio.AddHours(2));

        Assert.False(ticket.EstaVencido(Inicio.AddDays(10)));
    }
}
