using FluentValidation;

namespace Citas.Application.Medicos;

/// <summary>Campos comunes a crear y actualizar médico, para reutilizar validaciones.</summary>
public interface IDatosMedico
{
    string Nombres { get; }
    string Apellidos { get; }
    string Especialidad { get; }
    string NumeroColegiacion { get; }
    string? Telefono { get; }
    string? Correo { get; }
}

public sealed class DatosMedicoValidator : AbstractValidator<IDatosMedico>
{
    public DatosMedicoValidator()
    {
        RuleFor(x => x.Nombres).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Especialidad).NotEmpty().MaximumLength(100);
        RuleFor(x => x.NumeroColegiacion).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Telefono).MaximumLength(20);
        RuleFor(x => x.Correo).EmailAddress().MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));
    }
}
