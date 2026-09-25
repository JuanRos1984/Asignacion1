namespace MesaAyuda.Api.Errores;

public sealed class TicketNoEncontradoException(int numero)
    : Exception($"No existe el ticket {numero}.");
