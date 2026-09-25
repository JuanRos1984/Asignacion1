using MesaAyuda.Api.Dominio;

namespace MesaAyuda.Tests;

public class MaquinaEstadosTicketTests
{
    [Theory]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.EnProceso)]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.Cancelado)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.Cancelado)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.EnProceso)]
    public void Permite_las_transiciones_de_la_tabla(EstadoTicket desde, EstadoTicket hacia)
    {
        Assert.True(MaquinaEstadosTicket.PuedeTransicionar(desde, hacia));
    }

    [Theory]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.Resuelto)]
    [InlineData(EstadoTicket.Abierto, EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Resuelto, EstadoTicket.Cancelado)]
    [InlineData(EstadoTicket.EnProceso, EstadoTicket.EnProceso)]
    public void Rechaza_las_transiciones_fuera_de_la_tabla(EstadoTicket desde, EstadoTicket hacia)
    {
        Assert.False(MaquinaEstadosTicket.PuedeTransicionar(desde, hacia));
    }

    [Theory]
    [InlineData(EstadoTicket.Cerrado)]
    [InlineData(EstadoTicket.Cancelado)]
    public void Los_estados_terminales_no_tienen_salida(EstadoTicket terminal)
    {
        Assert.True(MaquinaEstadosTicket.EsTerminal(terminal));
        foreach (var destino in Enum.GetValues<EstadoTicket>())
            Assert.False(MaquinaEstadosTicket.PuedeTransicionar(terminal, destino));
    }

    [Fact]
    public void Resolver_y_cancelar_exigen_nota()
    {
        Assert.True(MaquinaEstadosTicket.ExigeNota(EstadoTicket.Resuelto));
        Assert.True(MaquinaEstadosTicket.ExigeNota(EstadoTicket.Cancelado));
        Assert.False(MaquinaEstadosTicket.ExigeNota(EstadoTicket.EnProceso));
    }
}
