using Citas.Application.Abstractions;
using Citas.Application.Citas;
using Citas.Application.Exceptions;
using Citas.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Medicos;

/// <summary>Citas activas (no canceladas) de un médico en un día específico.</summary>
public sealed record AgendaMedicoQuery(Guid MedicoId, DateOnly Fecha) : IRequest<AgendaMedicoDto>;

public sealed record AgendaMedicoDto(MedicoDto Medico, DateOnly Fecha, IReadOnlyList<CitaDto> Citas);

public sealed class AgendaMedicoHandler(IAppDbContext db) : IRequestHandler<AgendaMedicoQuery, AgendaMedicoDto>
{
    public async Task<AgendaMedicoDto> Handle(AgendaMedicoQuery r, CancellationToken ct)
    {
        var medico = await db.Medicos.AsNoTracking()
                         .Where(m => m.Id == r.MedicoId)
                         .Select(MedicoMappings.Proyeccion)
                         .FirstOrDefaultAsync(ct)
                     ?? throw new NotFoundException("Médico", r.MedicoId);

        var inicioDia = r.Fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        var finDia = inicioDia.AddDays(1);

        var citas = await db.Citas.AsNoTracking()
            .Where(c => c.MedicoId == r.MedicoId
                        && c.FechaHoraInicio >= inicioDia
                        && c.FechaHoraInicio < finDia
                        && c.Estado != EstadoCita.Cancelada)
            .OrderBy(c => c.FechaHoraInicio)
            .Select(CitaMappings.Proyeccion)
            .ToListAsync(ct);

        return new AgendaMedicoDto(medico, r.Fecha, citas);
    }
}
