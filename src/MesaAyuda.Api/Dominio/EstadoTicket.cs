namespace MesaAyuda.Api.Dominio;

/// <summary>Estados por los que pasa un ticket. Cerrado y Cancelado son terminales.</summary>
public enum EstadoTicket
{
    Abierto,
    EnProceso,
    Resuelto,
    Cerrado,
    Cancelado
}
