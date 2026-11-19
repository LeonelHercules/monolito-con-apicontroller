using Citas.Application.Abstractions;
using Citas.Application.Exceptions;
using Citas.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Medicos;

public sealed record ActualizarMedicoCommand(
    Guid Id, string Nombres, string Apellidos, string Especialidad,
    string NumeroColegiacion, string? Telefono, string? Correo) : IRequest<MedicoDto>, IDatosMedico;

public sealed class ActualizarMedicoValidator : AbstractValidator<ActualizarMedicoCommand>
{
    public ActualizarMedicoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        Include(new DatosMedicoValidator());
    }
}

public sealed class ActualizarMedicoHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<ActualizarMedicoCommand, MedicoDto>
{
    public async Task<MedicoDto> Handle(ActualizarMedicoCommand r, CancellationToken ct)
    {
        var medico = await db.Medicos.FirstOrDefaultAsync(m => m.Id == r.Id, ct)
                     ?? throw new NotFoundException("Médico", r.Id);

        var colegiacion = Medico.NormalizarColegiacion(r.NumeroColegiacion);
        if (await db.Medicos.AnyAsync(m => m.Id != r.Id && m.NumeroColegiacion == colegiacion, ct))
            throw new ConflictException($"Ya existe otro médico con el número de colegiación '{colegiacion}'.");

        medico.Actualizar(r.Nombres, r.Apellidos, r.Especialidad,
            r.NumeroColegiacion, r.Telefono, r.Correo, clock.AhoraUtc());

        await db.SaveChangesAsync(ct);
        return medico.ToDto();
    }
}
