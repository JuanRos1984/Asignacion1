namespace MesaAyuda.Tests;

/// <summary>Reloj controlado por la prueba: la hora solo cambia cuando se llama a Avanzar.</summary>
public sealed class RelojFijo(DateTimeOffset inicio) : TimeProvider
{
    private DateTimeOffset _ahora = inicio;

    public override DateTimeOffset GetUtcNow() => _ahora;

    public void Avanzar(TimeSpan tiempo) => _ahora += tiempo;
}
