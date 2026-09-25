using MesaAyuda.Api.Dominio;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MesaAyuda.Api.Errores;

/// <summary>
/// Traduce cada excepción a una respuesta ProblemDetails. Ningún mensaje hacia el cliente
/// lleva trazas, rutas ni detalles internos; esos quedan solo en el log.
/// </summary>
public sealed class ManejadorErrores(IProblemDetailsService problemas, ILogger<ManejadorErrores> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken ct)
    {
        ProblemDetails detalle = excepcion switch
        {
            ValidacionException v => new ValidationProblemDetails(v.Errores)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "La solicitud tiene datos inválidos."
            },
            BadHttpRequestException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "La petición no tiene un formato válido.",
                Detail = "Revisa el cuerpo JSON y los parámetros de la consulta: algún valor no tiene el tipo esperado."
            },
            TicketNoEncontradoException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Ticket no encontrado.",
                Detail = excepcion.Message
            },
            TransicionInvalidaException t => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Transición de estado no permitida.",
                Detail = t.Message,
                Extensions = { ["permitidas"] = MaquinaEstadosTicket.SiguientesDesde(t.Desde) }
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Ocurrió un error inesperado.",
                Detail = "El incidente quedó registrado. Si se repite, comunica el identificador de correlación."
            }
        };

        if (detalle.Status == StatusCodes.Status500InternalServerError)
            logger.LogError(excepcion, "Error no controlado en {Ruta}", contexto.Request.Path);
        else
            logger.LogWarning("Solicitud rechazada ({Estado}): {Mensaje}", detalle.Status, excepcion.Message);

        contexto.Response.StatusCode = detalle.Status!.Value;
        return await problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            ProblemDetails = detalle,
            Exception = excepcion
        });
    }
}
