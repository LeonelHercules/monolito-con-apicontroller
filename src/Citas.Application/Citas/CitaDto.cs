using System.Linq.Expressions;
using Citas.Domain.Entities;
using Citas.Domain.Enums;

namespace Citas.Application.Citas;

public sealed record CitaDto(
    Guid Id,
    Guid MedicoId,
    string MedicoNombre,
    string MedicoEspecialidad,
    string PacienteNombre,
    string PacienteIdentidad,
    string? PacienteTelefono,
    string? PacienteCorreo,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    int DuracionMinutos,
    string Motivo,
    string? Notas,
    EstadoCita Estado,
    string? MotivoCancelacion,
    DateTime FechaCreacion);

public static class CitaMappings
{
    public static readonly Expression<Func<Cita, CitaDto>> Proyeccion = c => new CitaDto(
        c.Id, c.MedicoId,
        c.Medico.Nombres + " " + c.Medico.Apellidos, c.Medico.Especialidad,
        c.PacienteNombre, c.PacienteIdentidad, c.PacienteTelefono, c.PacienteCorreo,
        c.FechaHoraInicio, c.FechaHoraFin,
        (int)(c.FechaHoraFin - c.FechaHoraInicio).TotalMinutes,
        c.Motivo, c.Notas, c.Estado, c.MotivoCancelacion, c.FechaCreacion);

    private static readonly Func<Cita, CitaDto> Compilada = Proyeccion.Compile();

    /// <summary>Requiere que la navegación Medico esté cargada.</summary>
    public static CitaDto ToDto(this Cita cita) => Compilada(cita);
}
