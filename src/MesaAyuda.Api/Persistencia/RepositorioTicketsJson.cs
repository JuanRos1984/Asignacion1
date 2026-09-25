using System.Text.Json;
using System.Text.Json.Serialization;
using MesaAyuda.Api.Dominio;

namespace MesaAyuda.Api.Persistencia;

/// <summary>
/// Guarda los tickets en un archivo JSON. Cada operación lee y escribe el archivo completo
/// bajo un candado, y la escritura pasa por un archivo temporal para no dejar el JSON
/// a medias si el proceso se interrumpe.
/// </summary>
public sealed class RepositorioTicketsJson : IRepositorioTickets
{
    private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _ruta;
    private readonly SemaphoreSlim _candado = new(1, 1);

    public RepositorioTicketsJson(string ruta)
    {
        _ruta = ruta;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_ruta))!);
    }

    public async Task<IReadOnlyList<Ticket>> ListarAsync(CancellationToken ct = default)
    {
        await _candado.WaitAsync(ct);
        try { return await LeerAsync(ct); }
        finally { _candado.Release(); }
    }

    public async Task<Ticket?> ObtenerAsync(int numero, CancellationToken ct = default) =>
        (await ListarAsync(ct)).FirstOrDefault(t => t.Numero == numero);

    public async Task<Ticket> AgregarAsync(Ticket ticket, CancellationToken ct = default)
    {
        await _candado.WaitAsync(ct);
        try
        {
            var tickets = await LeerAsync(ct);
            ticket.Numero = tickets.Count == 0 ? 1 : tickets.Max(t => t.Numero) + 1;
            tickets.Add(ticket);
            await EscribirAsync(tickets, ct);
            return ticket;
        }
        finally { _candado.Release(); }
    }

    public async Task ActualizarAsync(Ticket ticket, CancellationToken ct = default)
    {
        await _candado.WaitAsync(ct);
        try
        {
            var tickets = await LeerAsync(ct);
            var indice = tickets.FindIndex(t => t.Numero == ticket.Numero);
            if (indice < 0)
                throw new KeyNotFoundException($"No existe el ticket {ticket.Numero}.");

            tickets[indice] = ticket;
            await EscribirAsync(tickets, ct);
        }
        finally { _candado.Release(); }
    }

    private async Task<List<Ticket>> LeerAsync(CancellationToken ct)
    {
        if (!File.Exists(_ruta))
            return [];

        await using var flujo = File.OpenRead(_ruta);
        try
        {
            return await JsonSerializer.DeserializeAsync<List<Ticket>>(flujo, OpcionesJson, ct) ?? [];
        }
        catch (JsonException ex)
        {
            // No se sobrescribe un archivo dañado: se detiene la operación para no perder datos.
            throw new InvalidOperationException($"El archivo de datos '{_ruta}' no es un JSON válido.", ex);
        }
    }

    private async Task EscribirAsync(List<Ticket> tickets, CancellationToken ct)
    {
        var temporal = _ruta + ".tmp";
        await using (var flujo = File.Create(temporal))
        {
            await JsonSerializer.SerializeAsync(flujo, tickets, OpcionesJson, ct);
        }
        File.Move(temporal, _ruta, overwrite: true);
    }
}
