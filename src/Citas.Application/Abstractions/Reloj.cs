namespace Citas.Application.Abstractions;

/// <summary>
/// Atajos sobre TimeProvider: UTC para auditoría y hora local (de la clínica) para la agenda.
/// </summary>
public static class Reloj
{
    public static DateTime AhoraUtc(this TimeProvider clock) => clock.GetUtcNow().UtcDateTime;

    public static DateTime AhoraLocal(this TimeProvider clock) =>
        DateTime.SpecifyKind(clock.GetLocalNow().DateTime, DateTimeKind.Unspecified);

    /// <summary>Las fechas de agenda se tratan como hora local sin zona.</summary>
    public static DateTime ComoLocal(this DateTime fecha) =>
        DateTime.SpecifyKind(fecha, DateTimeKind.Unspecified);
}
