using Citas.Application.Abstractions;
using Citas.Application.Exceptions;
using Citas.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Citas;

public sealed record ProgramarCitaCommand(
    Guid MedicoId,
    string PacienteNombre,
    string PacienteIdentidad,
    string? PacienteTelefono,
    string? PacienteCorreo,
    DateTime FechaHoraInicio,
    int DuracionMinutos,
    string Motivo,
    string? Notas) : IRequest<CitaDto>;

public sealed class ProgramarCitaValidator : AbstractValidator<ProgramarCitaCommand>
{
    public ProgramarCitaValidator()
    {
        RuleFor(x => x.MedicoId).NotEmpty();
        RuleFor(x => x.PacienteNombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PacienteIdentidad).NotEmpty()
            .Matches(@"^\d{4}-?\d{4}-?\d{5}$")
            .WithMessage("La identidad debe tener 13 dígitos (ej. 0801-1990-12345).");
        RuleFor(x => x.PacienteTelefono).MaximumLength(20);
        RuleFor(x => x.PacienteCorreo).EmailAddress().MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.PacienteCorreo));
        RuleFor(x => x.FechaHoraInicio).NotEmpty();
        RuleFor(x => x.DuracionMinutos).InclusiveBetween(Cita.DuracionMinima, Cita.DuracionMaxima);
        RuleFor(x => x.Motivo).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Notas).MaximumLength(2000);
    }
}

public sealed class ProgramarCitaHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<ProgramarCitaCommand, CitaDto>
{
    public async Task<CitaDto> Handle(ProgramarCitaCommand r, CancellationToken ct)
    {
        var medico = await db.Medicos.FirstOrDefaultAsync(m => m.Id == r.MedicoId, ct)
                     ?? throw new NotFoundException("Médico", r.MedicoId);

        if (!medico.Activo)
            throw new ConflictException("El médico está inactivo y no puede recibir citas.");

        var cita = Cita.Programar(
            r.MedicoId, r.PacienteNombre, r.PacienteIdentidad, r.PacienteTelefono, r.PacienteCorreo,
            r.FechaHoraInicio.ComoLocal(), r.DuracionMinutos, r.Motivo, r.Notas,
            clock.AhoraLocal(), clock.AhoraUtc());

        await ReglasAgenda.ValidarDisponibilidadAsync(db, cita, ct);

        db.Citas.Add(cita);
        await db.SaveChangesAsync(ct);
        return cita.ToDto(); // el médico está trackeado: EF enlaza la navegación
    }
}
