using Citas.Application.Abstractions;
using Citas.Domain.Entities;
using FluentValidation;
using MediatR;

namespace Citas.Application.Citas;

public sealed record ReprogramarCitaCommand(Guid Id, DateTime FechaHoraInicio, int DuracionMinutos)
    : IRequest<CitaDto>;

public sealed class ReprogramarCitaValidator : AbstractValidator<ReprogramarCitaCommand>
{
    public ReprogramarCitaValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FechaHoraInicio).NotEmpty();
        RuleFor(x => x.DuracionMinutos).InclusiveBetween(Cita.DuracionMinima, Cita.DuracionMaxima);
    }
}

public sealed class ReprogramarCitaHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<ReprogramarCitaCommand, CitaDto>
{
    public async Task<CitaDto> Handle(ReprogramarCitaCommand r, CancellationToken ct)
    {
        var cita = await ReglasAgenda.ObtenerConMedicoAsync(db, r.Id, ct);

        cita.Reprogramar(r.FechaHoraInicio.ComoLocal(), r.DuracionMinutos, clock.AhoraLocal(), clock.AhoraUtc());
        await ReglasAgenda.ValidarDisponibilidadAsync(db, cita, ct);

        await db.SaveChangesAsync(ct);
        return cita.ToDto();
    }
}
