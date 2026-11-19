using System.Linq.Expressions;
using Citas.Domain.Entities;

namespace Citas.Application.Medicos;

public sealed record MedicoDto(
    Guid Id,
    string Nombres,
    string Apellidos,
    string NombreCompleto,
    string Especialidad,
    string NumeroColegiacion,
    string? Telefono,
    string? Correo,
    bool Activo,
    DateTime FechaCreacion);

public static class MedicoMappings
{
    public static readonly Expression<Func<Medico, MedicoDto>> Proyeccion = m => new MedicoDto(
        m.Id, m.Nombres, m.Apellidos, m.Nombres + " " + m.Apellidos,
        m.Especialidad, m.NumeroColegiacion, m.Telefono, m.Correo, m.Activo, m.FechaCreacion);

    private static readonly Func<Medico, MedicoDto> Compilada = Proyeccion.Compile();

    public static MedicoDto ToDto(this Medico medico) => Compilada(medico);
}
