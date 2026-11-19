using Citas.Application.Abstractions;
using Citas.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Medicos;

public sealed record ObtenerMedicoQuery(Guid Id) : IRequest<MedicoDto>;

public sealed class ObtenerMedicoHandler(IAppDbContext db) : IRequestHandler<ObtenerMedicoQuery, MedicoDto>
{
    public async Task<MedicoDto> Handle(ObtenerMedicoQuery r, CancellationToken ct) =>
        await db.Medicos.AsNoTracking()
            .Where(m => m.Id == r.Id)
            .Select(MedicoMappings.Proyeccion)
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("Médico", r.Id);
}
