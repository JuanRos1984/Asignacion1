namespace MesaAyuda.Api.Persistencia;

public sealed class OpcionesAlmacenamiento
{
    public const string Seccion = "Almacenamiento";

    /// <summary>Ruta del archivo JSON. Si es relativa, se resuelve desde la raíz del contenido.</summary>
    public string RutaDatos { get; set; } = "data/tickets.json";
}
