using Citas.Application.Abstractions;
using Citas.Application.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Medicos;

public sealed record ListarMedicosQuery(
    string? Buscar, string? Especialidad, bool? Activo, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<MedicoDto>>;

public sealed class ListarMedicosValidator : AbstractValidator<ListarMedicosQuery>
{
    public ListarMedicosValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class ListarMedicosHandler(IAppDbContext db)
    : IRequestHandler<ListarMedicosQuery, PagedResult<MedicoDto>>
{
    public async Task<PagedResult<MedicoDto>> Handle(ListarMedicosQuery r, CancellationToken ct)
    {
        var query = db.Medicos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(r.Buscar))
        {
            var texto = r.Buscar.Trim().ToLower();
            query = query.Where(m =>
                (m.Nombres + " " + m.Apellidos).ToLower().Contains(texto) ||
                m.NumeroColegiacion.ToLower().Contains(texto));
        }

        if (!string.IsNullOrWhiteSpace(r.Especialidad))
        {
            var esp = r.Especialidad.Trim().ToLower();
            query = query.Where(m => m.Especialidad.ToLower() == esp);
        }

        if (r.Activo.HasValue)
            query = query.Where(m => m.Activo == r.Activo.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.Apellidos).ThenBy(m => m.Nombres)
            .Skip((r.Page - 1) * r.PageSize).Take(r.PageSize)
            .Select(MedicoMappings.Proyeccion)
            .ToListAsync(ct);

        return new PagedResult<MedicoDto>(items, r.Page, r.PageSize, total);
    }
}
