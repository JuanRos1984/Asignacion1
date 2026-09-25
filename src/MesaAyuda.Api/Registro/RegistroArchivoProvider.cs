using System.Collections.Concurrent;
using System.Text;

namespace MesaAyuda.Api.Registro;

/// <summary>
/// Escribe el log en archivos de texto, uno por día (mesa-ayuda-AAAAMMDD.txt).
/// Todas las categorías comparten el mismo escritor para no mezclar líneas.
/// </summary>
public sealed class RegistroArchivoProvider(string carpeta, LogLevel nivelMinimo, TimeProvider reloj) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, RegistroArchivo> _registros = new();
    private readonly Lock _candado = new();

    public ILogger CreateLogger(string categoria) =>
        _registros.GetOrAdd(categoria, c => new RegistroArchivo(c, this));

    internal bool EstaHabilitado(LogLevel nivel) => nivel != LogLevel.None && nivel >= nivelMinimo;

    internal void Escribir(string categoria, LogLevel nivel, string mensaje, Exception? excepcion)
    {
        var ahora = reloj.GetUtcNow();
        var linea = new StringBuilder()
            .Append(ahora.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
            .Append(" [").Append(Abreviar(nivel)).Append("] ")
            .Append(categoria).Append(": ")
            .Append(mensaje);
        if (excepcion is not null)
            linea.AppendLine().Append(excepcion);
        linea.AppendLine();

        var archivo = Path.Combine(carpeta, $"mesa-ayuda-{ahora:yyyyMMdd}.txt");
        lock (_candado)
        {
            Directory.CreateDirectory(carpeta);
            File.AppendAllText(archivo, linea.ToString());
        }
    }

    private static string Abreviar(LogLevel nivel) => nivel switch
    {
        LogLevel.Trace => "TRC",
        LogLevel.Debug => "DBG",
        LogLevel.Information => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error => "ERR",
        _ => "CRT"
    };

    public void Dispose() => _registros.Clear();

    private sealed class RegistroArchivo(string categoria, RegistroArchivoProvider proveedor) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel nivel) => proveedor.EstaHabilitado(nivel);

        public void Log<TState>(LogLevel nivel, EventId eventId, TState state, Exception? excepcion,
            Func<TState, Exception?, string> formateador)
        {
            if (!IsEnabled(nivel))
                return;
            proveedor.Escribir(categoria, nivel, formateador(state, excepcion), excepcion);
        }
    }
}
