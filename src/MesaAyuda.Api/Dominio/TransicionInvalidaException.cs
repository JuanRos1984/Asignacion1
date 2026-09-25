namespace MesaAyuda.Api.Dominio;

public sealed class TransicionInvalidaException(EstadoTicket desde, EstadoTicket hacia)
    : Exception($"No se permite pasar un ticket de {desde} a {hacia}.")
{
    public EstadoTicket Desde { get; } = desde;
    public EstadoTicket Hacia { get; } = hacia;
}
