using Citas.Application.Abstractions;
using Citas.Application.Common;
using Citas.Domain.Entities;
using Citas.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Citas;

public sealed record ListarCitasQuery(
    Guid? MedicoId,
    EstadoCita? Estado,
    DateTime? Desde,
    DateTime? Hasta,
    string? PacienteIdentidad,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<CitaDto>>;

public sealed class ListarCitasValidator : AbstractValidator<ListarCitasQuery>
{
    public ListarCitasValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Desde)
            .Must((q, desde) => desde <= q.Hasta)
            .When(x => x.Desde.HasValue && x.Hasta.HasValue)
            .WithMessage("'Desde' debe ser menor o igual que 'Hasta'.");
    }
}

public sealed class ListarCitasHandler(IAppDbContext db)
    : IRequestHandler<ListarCitasQuery, PagedResult<CitaDto>>
{
    public async Task<PagedResult<CitaDto>> Handle(ListarCitasQuery r, CancellationToken ct)
    {
        var query = db.Citas.AsNoTracking();

        if (r.MedicoId.HasValue) query = query.Where(c => c.MedicoId == r.MedicoId);
        if (r.Estado.HasValue) query = query.Where(c => c.Estado == r.Estado);

        if (r.Desde.HasValue)
        {
            var desde = r.Desde.Value.ComoLocal();
            query = query.Where(c => c.FechaHoraInicio >= desde);
        }
        if (r.Hasta.HasValue)
        {
            var hasta = r.Hasta.Value.ComoLocal();
            query = query.Where(c => c.FechaHoraInicio <= hasta);
        }
        if (!string.IsNullOrWhiteSpace(r.PacienteIdentidad))
        {
            var identidad = Cita.NormalizarIdentidad(r.PacienteIdentidad);
            query = query.Where(c => c.PacienteIdentidad == identidad);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.FechaHoraInicio)
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize)
            .Select(CitaMappings.Proyeccion)
            .ToListAsync(ct);

        return new PagedResult<CitaDto>(items, r.Page, r.PageSize, total);
    }
}
