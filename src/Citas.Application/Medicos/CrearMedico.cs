using Citas.Application.Abstractions;
using Citas.Application.Exceptions;
using Citas.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Medicos;

public sealed record CrearMedicoCommand(
    string Nombres, string Apellidos, string Especialidad,
    string NumeroColegiacion, string? Telefono, string? Correo) : IRequest<MedicoDto>, IDatosMedico;

public sealed class CrearMedicoValidator : AbstractValidator<CrearMedicoCommand>
{
    public CrearMedicoValidator() => Include(new DatosMedicoValidator());
}

public sealed class CrearMedicoHandler(IAppDbContext db, TimeProvider clock)
    : IRequestHandler<CrearMedicoCommand, MedicoDto>
{
    public async Task<MedicoDto> Handle(CrearMedicoCommand r, CancellationToken ct)
    {
        var colegiacion = Medico.NormalizarColegiacion(r.NumeroColegiacion);
        if (await db.Medicos.AnyAsync(m => m.NumeroColegiacion == colegiacion, ct))
            throw new ConflictException($"Ya existe un médico con el número de colegiación '{colegiacion}'.");

        var medico = Medico.Crear(r.Nombres, r.Apellidos, r.Especialidad,
            r.NumeroColegiacion, r.Telefono, r.Correo, clock.AhoraUtc());

        db.Medicos.Add(medico);
        await db.SaveChangesAsync(ct);
        return medico.ToDto();
    }
}
