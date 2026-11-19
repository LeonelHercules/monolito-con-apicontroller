using Citas.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace Citas.Application.Citas;

// ---------- Confirmar ----------
public sealed record ConfirmarCitaCommand(Guid Id) : IRequest<CitaDto>;

public sealed class ConfirmarCitaHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<ConfirmarCitaCommand, CitaDto>
{
    public async Task<CitaDto> Handle(ConfirmarCitaCommand r, CancellationToken ct)
    {
        var cita = await ReglasAgenda.ObtenerConMedicoAsync(db, r.Id, ct);
        cita.Confirmar(clock.AhoraUtc());
        await db.SaveChangesAsync(ct);
        return cita.ToDto();
    }
}

// ---------- Cancelar ----------
public sealed record CancelarCitaCommand(Guid Id, string Motivo) : IRequest<CitaDto>;

public sealed class CancelarCitaValidator : AbstractValidator<CancelarCitaCommand>
{
    public CancelarCitaValidator() => RuleFor(x => x.Motivo).NotEmpty().MaximumLength(500);
}

public sealed class CancelarCitaHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<CancelarCitaCommand, CitaDto>
{
    public async Task<CitaDto> Handle(CancelarCitaCommand r, CancellationToken ct)
    {
        var cita = await ReglasAgenda.ObtenerConMedicoAsync(db, r.Id, ct);
        cita.Cancelar(r.Motivo, clock.AhoraUtc());
        await db.SaveChangesAsync(ct);
        return cita.ToDto();
    }
}

// ---------- Completar ----------
public sealed record CompletarCitaCommand(Guid Id, string? Notas) : IRequest<CitaDto>;

public sealed class CompletarCitaValidator : AbstractValidator<CompletarCitaCommand>
{
    public CompletarCitaValidator() => RuleFor(x => x.Notas).MaximumLength(2000);
}

public sealed class CompletarCitaHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<CompletarCitaCommand, CitaDto>
{
    public async Task<CitaDto> Handle(CompletarCitaCommand r, CancellationToken ct)
    {
        var cita = await ReglasAgenda.ObtenerConMedicoAsync(db, r.Id, ct);
        cita.Completar(r.Notas, clock.AhoraLocal(), clock.AhoraUtc());
        await db.SaveChangesAsync(ct);
        return cita.ToDto();
    }
}

// ---------- No asistió ----------
public sealed record MarcarNoAsistioCommand(Guid Id) : IRequest<CitaDto>;

public sealed class MarcarNoAsistioHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<MarcarNoAsistioCommand, CitaDto>
{
    public async Task<CitaDto> Handle(MarcarNoAsistioCommand r, CancellationToken ct)
    {
        var cita = await ReglasAgenda.ObtenerConMedicoAsync(db, r.Id, ct);
        cita.MarcarNoAsistio(clock.AhoraLocal(), clock.AhoraUtc());
        await db.SaveChangesAsync(ct);
        return cita.ToDto();
    }
}
