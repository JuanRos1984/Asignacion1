using System.Diagnostics;

namespace MesaAyuda.Api.Registro;

/// <summary>
/// Registra cada petición con su resultado y duración. Si la petición trae el encabezado
/// X-Id-Correlacion lo reutiliza; si no, genera uno y lo devuelve en la respuesta.
/// </summary>
public sealed class RegistroPeticionesMiddleware(RequestDelegate siguiente, ILogger<RegistroPeticionesMiddleware> logger)
{
    public const string Encabezado = "X-Id-Correlacion";

    public async Task InvokeAsync(HttpContext contexto)
    {
        var idCorrelacion = contexto.Request.Headers[Encabezado].FirstOrDefault() ?? Guid.NewGuid().ToString("N")[..12];
        contexto.Response.Headers[Encabezado] = idCorrelacion;

        var cronometro = Stopwatch.StartNew();
        try
        {
            await siguiente(contexto);
        }
        finally
        {
            cronometro.Stop();
            logger.LogInformation("{Id} {Metodo} {Ruta} -> {Estado} en {Ms} ms",
                idCorrelacion, contexto.Request.Method, contexto.Request.Path + contexto.Request.QueryString,
                contexto.Response.StatusCode, cronometro.ElapsedMilliseconds);
        }
    }
}
