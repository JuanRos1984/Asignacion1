using MesaAyuda.Api.Dominio;

namespace MesaAyuda.Api.Persistencia;

public interface IRepositorioTickets
{
    Task<IReadOnlyList<Ticket>> ListarAsync(CancellationToken ct = default);
    Task<Ticket?> ObtenerAsync(int numero, CancellationToken ct = default);

    /// <summary>Guarda un ticket nuevo y le asigna el siguiente número consecutivo.</summary>
    Task<Ticket> AgregarAsync(Ticket ticket, CancellationToken ct = default);

    Task ActualizarAsync(Ticket ticket, CancellationToken ct = default);
}
