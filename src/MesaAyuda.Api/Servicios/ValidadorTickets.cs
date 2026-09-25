using System.Net.Mail;
using MesaAyuda.Api.Contratos;
using MesaAyuda.Api.Dominio;
using MesaAyuda.Api.Errores;

namespace MesaAyuda.Api.Servicios;

/// <summary>Toda entrada externa pasa por aquí antes de llegar al dominio.</summary>
public static class ValidadorTickets
{
    public const int TamanoMaximoPagina = 50;

    public static void Validar(CrearTicketSolicitud s)
    {
        var errores = new Dictionary<string, string[]>();

        Texto(errores, "titulo", s.Titulo, 5, 120);
        Texto(errores, "descripcion", s.Descripcion, 10, 2000);

        if (string.IsNullOrWhiteSpace(s.Solicitante))
            errores["solicitante"] = ["El correo del solicitante es obligatorio."];
        else if (!EsCorreo(s.Solicitante.Trim()))
            errores["solicitante"] = ["El correo del solicitante no tiene un formato válido."];

        if (s.Prioridad is null)
            errores["prioridad"] = ["La prioridad es obligatoria."];
        else if (!Enum.IsDefined(s.Prioridad.Value))
            errores["prioridad"] = ["La prioridad no existe."];

        Lanzar(errores);
    }

    public static void Validar(AsignarTecnicoSolicitud s)
    {
        var errores = new Dictionary<string, string[]>();
        Texto(errores, "tecnico", s.Tecnico, 3, 80);
        Lanzar(errores);
    }

    public static void Validar(CambiarEstadoSolicitud s)
    {
        var errores = new Dictionary<string, string[]>();

        if (s.Estado is null || !Enum.IsDefined(s.Estado.Value))
            errores["estado"] = ["Indica un estado válido."];
        else if (MaquinaEstadosTicket.ExigeNota(s.Estado.Value) && string.IsNullOrWhiteSpace(s.Nota))
            errores["nota"] = [$"Pasar a {s.Estado} exige una nota que explique el motivo."];

        if (s.Nota is { Length: > 500 })
            errores["nota"] = ["La nota no puede pasar de 500 caracteres."];

        Lanzar(errores);
    }

    public static void Validar(FiltroTickets f)
    {
        var errores = new Dictionary<string, string[]>();
        if (f.Pagina is < 1)
            errores["pagina"] = ["La página empieza en 1."];
        if (f.Tamano is < 1 or > TamanoMaximoPagina)
            errores["tamano"] = [$"El tamaño de página va de 1 a {TamanoMaximoPagina}."];
        Lanzar(errores);
    }

    private static void Texto(Dictionary<string, string[]> errores, string campo, string? valor, int minimo, int maximo)
    {
        var limpio = valor?.Trim() ?? "";
        if (limpio.Length == 0)
            errores[campo] = [$"El campo {campo} es obligatorio."];
        else if (limpio.Length < minimo || limpio.Length > maximo)
            errores[campo] = [$"El campo {campo} debe tener entre {minimo} y {maximo} caracteres."];
    }

    private static bool EsCorreo(string valor) =>
        MailAddress.TryCreate(valor, out var correo) && correo.Address == valor;

    private static void Lanzar(Dictionary<string, string[]> errores)
    {
        if (errores.Count > 0)
            throw new ValidacionException(errores);
    }
}
