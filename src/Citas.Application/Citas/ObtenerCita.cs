using Citas.Application.Abstractions;
using Citas.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Citas;

public sealed record ObtenerCitaQuery(Guid Id) : IRequest<CitaDto>;

public sealed class ObtenerCitaHandler(IAppDbContext db) : IRequestHandler<ObtenerCitaQuery, CitaDto>
{
    public async Task<CitaDto> Handle(ObtenerCitaQuery r, CancellationToken ct) =>
        await db.Citas.AsNoTracking()
            .Where(c => c.Id == r.Id)
            .Select(CitaMappings.Proyeccion)
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("Cita", r.Id);
}
