namespace MesaAyuda.Api.Registro;

public sealed class OpcionesRegistroArchivo
{
    public const string Seccion = "RegistroArchivo";

    /// <summary>Carpeta de los archivos de log. Si es relativa, se resuelve desde la raíz del contenido.</summary>
    public string Carpeta { get; set; } = "logs";

    public LogLevel NivelMinimo { get; set; } = LogLevel.Information;
}
