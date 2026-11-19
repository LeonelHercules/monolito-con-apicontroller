using Citas.Domain.Enums;
using Citas.Domain.Exceptions;

namespace Citas.Domain.Entities;

public sealed class Cita
{
    public const int DuracionMinima = 10;
    public const int DuracionMaxima = 240;

    public Guid Id { get; private set; }
    public Guid MedicoId { get; private set; }
    public Medico Medico { get; private set; } = default!;

    public string PacienteNombre { get; private set; } = default!;
    /// <summary>DNI hondureño normalizado a 13 dígitos, sin guiones.</summary>
    public string PacienteIdentidad { get; private set; } = default!;
    public string? PacienteTelefono { get; private set; }
    public string? PacienteCorreo { get; private set; }

    /// <summary>Hora local de la clínica.</summary>
    public DateTime FechaHoraInicio { get; private set; }
    public DateTime FechaHoraFin { get; private set; }

    public string Motivo { get; private set; } = default!;
    public string? Notas { get; private set; }
    public EstadoCita Estado { get; private set; }
    public string? MotivoCancelacion { get; private set; }

    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaActualizacion { get; private set; }

    public int DuracionMinutos => (int)(FechaHoraFin - FechaHoraInicio).TotalMinutes;

    private Cita() { } // EF Core

    public static Cita Programar(
        Guid medicoId, string pacienteNombre, string pacienteIdentidad,
        string? pacienteTelefono, string? pacienteCorreo,
        DateTime inicio, int duracionMinutos, string motivo, string? notas,
        DateTime ahoraLocal, DateTime ahoraUtc)
    {
        var cita = new Cita
        {
            Id = Guid.NewGuid(),
            MedicoId = medicoId,
            PacienteNombre = pacienteNombre.Trim(),
            PacienteIdentidad = NormalizarIdentidad(pacienteIdentidad),
            PacienteTelefono = Limpiar(pacienteTelefono),
            PacienteCorreo = Limpiar(pacienteCorreo)?.ToLowerInvariant(),
            Motivo = motivo.Trim(),
            Notas = Limpiar(notas),
            Estado = EstadoCita.Programada,
            FechaCreacion = ahoraUtc
        };
        cita.AsignarHorario(inicio, duracionMinutos, ahoraLocal);
        return cita;
    }

    public void Reprogramar(DateTime inicio, int duracionMinutos, DateTime ahoraLocal, DateTime ahoraUtc)
    {
        ValidarEstado("reprogramar", EstadoCita.Programada, EstadoCita.Confirmada);
        AsignarHorario(inicio, duracionMinutos, ahoraLocal);
        Estado = EstadoCita.Programada; // al reprogramar debe volver a confirmarse
        FechaActualizacion = ahoraUtc;
    }

    public void Confirmar(DateTime ahoraUtc)
    {
        ValidarEstado("confirmar", EstadoCita.Programada);
        Estado = EstadoCita.Confirmada;
        FechaActualizacion = ahoraUtc;
    }

    public void Cancelar(string motivo, DateTime ahoraUtc)
    {
        ValidarEstado("cancelar", EstadoCita.Programada, EstadoCita.Confirmada);
        Estado = EstadoCita.Cancelada;
        MotivoCancelacion = motivo.Trim();
        FechaActualizacion = ahoraUtc;
    }

    public void Completar(string? notas, DateTime ahoraLocal, DateTime ahoraUtc)
    {
        ValidarEstado("completar", EstadoCita.Programada, EstadoCita.Confirmada);
        if (ahoraLocal < FechaHoraInicio)
            throw new DomainException("No se puede completar una cita que aún no ha iniciado.");

        Estado = EstadoCita.Completada;
        if (!string.IsNullOrWhiteSpace(notas)) Notas = notas.Trim();
        FechaActualizacion = ahoraUtc;
    }

    public void MarcarNoAsistio(DateTime ahoraLocal, DateTime ahoraUtc)
    {
        ValidarEstado("marcar como no asistida", EstadoCita.Programada, EstadoCita.Confirmada);
        if (ahoraLocal < FechaHoraInicio)
            throw new DomainException("No se puede marcar inasistencia antes de la hora de la cita.");

        Estado = EstadoCita.NoAsistio;
        FechaActualizacion = ahoraUtc;
    }

    public static string NormalizarIdentidad(string identidad) =>
        new(identidad.Where(char.IsDigit).ToArray());

    private void AsignarHorario(DateTime inicio, int duracionMinutos, DateTime ahoraLocal)
    {
        inicio = DateTime.SpecifyKind(inicio, DateTimeKind.Unspecified);

        if (inicio <= ahoraLocal)
            throw new DomainException("La cita no puede programarse en una fecha u hora pasada.");
        if (duracionMinutos is < DuracionMinima or > DuracionMaxima)
            throw new DomainException($"La duración debe estar entre {DuracionMinima} y {DuracionMaxima} minutos.");

        FechaHoraInicio = inicio;
        FechaHoraFin = inicio.AddMinutes(duracionMinutos);
    }

    private void ValidarEstado(string accion, params EstadoCita[] permitidos)
    {
        if (!permitidos.Contains(Estado))
            throw new DomainException($"No se puede {accion} una cita en estado '{Estado}'.");
    }

    private static string? Limpiar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
