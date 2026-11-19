using Citas.Application.Abstractions;
using Citas.Application.Exceptions;
using Citas.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Medicos;

public sealed record CambiarEstadoMedicoCommand(Guid Id, bool Activo) : IRequest<MedicoDto>;

public sealed class CambiarEstadoMedicoHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<CambiarEstadoMedicoCommand, MedicoDto>
{
    public async Task<MedicoDto> Handle(CambiarEstadoMedicoCommand r, CancellationToken ct)
    {
        var medico = await db.Medicos.FirstOrDefaultAsync(m => m.Id == r.Id, ct)
                     ?? throw new NotFoundException("Médico", r.Id);

        if (r.Activo)
        {
            medico.Activar(clock.AhoraUtc());
        }
        else
        {
            var ahora = clock.AhoraLocal();
            var tieneCitasPendientes = await db.Citas.AnyAsync(c =>
                c.MedicoId == r.Id &&
                c.FechaHoraInicio > ahora &&
                (c.Estado == EstadoCita.Programada || c.Estado == EstadoCita.Confirmada), ct);

            if (tieneCitasPendientes)
                throw new ConflictException("El médico tiene citas futuras pendientes. Cancélelas o reprográmelas antes de desactivarlo.");

            medico.Desactivar(clock.AhoraUtc());
        }

        await db.SaveChangesAsync(ct);
        return medico.ToDto();
    }
}
