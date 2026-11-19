using Citas.Domain.Exceptions;

namespace Citas.Domain.Entities;

public sealed class Medico
{
    public Guid Id { get; private set; }
    public string Nombres { get; private set; } = default!;
    public string Apellidos { get; private set; } = default!;
    public string Especialidad { get; private set; } = default!;
    /// <summary>Número de colegiación (CMH). Único por médico.</summary>
    public string NumeroColegiacion { get; private set; } = default!;
    public string? Telefono { get; private set; }
    public string? Correo { get; private set; }
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }
    public DateTime? FechaActualizacion { get; private set; }

    private readonly List<Cita> _citas = [];
    public IReadOnlyCollection<Cita> Citas => _citas.AsReadOnly();

    private Medico() { } // EF Core

    public static Medico Crear(
        string nombres, string apellidos, string especialidad,
        string numeroColegiacion, string? telefono, string? correo, DateTime ahoraUtc)
    {
        var medico = new Medico
        {
            Id = Guid.NewGuid(),
            Activo = true,
            FechaCreacion = ahoraUtc
        };
        medico.AsignarDatos(nombres, apellidos, especialidad, numeroColegiacion, telefono, correo);
        return medico;
    }

    public void Actualizar(
        string nombres, string apellidos, string especialidad,
        string numeroColegiacion, string? telefono, string? correo, DateTime ahoraUtc)
    {
        AsignarDatos(nombres, apellidos, especialidad, numeroColegiacion, telefono, correo);
        FechaActualizacion = ahoraUtc;
    }

    public void Activar(DateTime ahoraUtc)
    {
        if (Activo) throw new DomainException("El médico ya está activo.");
        Activo = true;
        FechaActualizacion = ahoraUtc;
    }

    public void Desactivar(DateTime ahoraUtc)
    {
        if (!Activo) throw new DomainException("El médico ya está inactivo.");
        Activo = false;
        FechaActualizacion = ahoraUtc;
    }

    public static string NormalizarColegiacion(string valor) => valor.Trim().ToUpperInvariant();

    private void AsignarDatos(
        string nombres, string apellidos, string especialidad,
        string numeroColegiacion, string? telefono, string? correo)
    {
        Nombres = nombres.Trim();
        Apellidos = apellidos.Trim();
        Especialidad = especialidad.Trim();
        NumeroColegiacion = NormalizarColegiacion(numeroColegiacion);
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
        Correo = string.IsNullOrWhiteSpace(correo) ? null : correo.Trim().ToLowerInvariant();
    }
}
