namespace MesaAyuda.Api.Errores;

/// <summary>Datos de entrada rechazados. Lleva los mensajes agrupados por campo.</summary>
public sealed class ValidacionException(IDictionary<string, string[]> errores)
    : Exception("La solicitud tiene datos inválidos.")
{
    public IDictionary<string, string[]> Errores { get; } = errores;
}
