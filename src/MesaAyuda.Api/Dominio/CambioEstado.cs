namespace MesaAyuda.Api.Dominio;

/// <summary>Entrada del historial de un ticket: de qué estado a cuál, cuándo y por qué.</summary>
public sealed record CambioEstado(EstadoTicket Desde, EstadoTicket Hacia, DateTimeOffset Fecha, string? Nota);
