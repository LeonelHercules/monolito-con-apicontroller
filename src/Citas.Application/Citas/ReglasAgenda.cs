using Citas.Application.Abstractions;
using Citas.Application.Exceptions;
using Citas.Domain.Entities;
using Citas.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Citas;

internal static class ReglasAgenda
{
    /// <summary>
    /// Valida que ni el médico ni el paciente tengan otra cita que se traslape con el horario.
    /// Dos rangos [a,b) y [c,d) se traslapan si a &lt; d y c &lt; b.
    /// </summary>
    public static async Task ValidarDisponibilidadAsync(IAppDbContext db, Cita cita, CancellationToken ct)
    {
        var activas = db.Citas.AsNoTracking().Where(c =>
            c.Id != cita.Id &&
            (c.Estado == EstadoCita.Programada || c.Estado == EstadoCita.Confirmada) &&
            c.FechaHoraInicio < cita.FechaHoraFin &&
            cita.FechaHoraInicio < c.FechaHoraFin);

        if (await activas.AnyAsync(c => c.MedicoId == cita.MedicoId, ct))
            throw new ConflictException("El médico ya tiene una cita en ese horario.");

        if (await activas.AnyAsync(c => c.PacienteIdentidad == cita.PacienteIdentidad, ct))
            throw new ConflictException("El paciente ya tiene otra cita que se traslapa con ese horario.");
    }

    public static async Task<Cita> ObtenerConMedicoAsync(IAppDbContext db, Guid id, CancellationToken ct) =>
        await db.Citas.Include(c => c.Medico).FirstOrDefaultAsync(c => c.Id == id, ct)
        ?? throw new NotFoundException("Cita", id);
}
