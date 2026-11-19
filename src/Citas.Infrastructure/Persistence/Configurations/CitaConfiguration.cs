using Citas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Citas.Infrastructure.Persistence.Configurations;

internal sealed class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> b)
    {
        b.ToTable("Citas", t =>
        {
            // En SQL Server los nombres de columna van entre corchetes
            t.HasCheckConstraint("CK_Citas_RangoHorario", "[FechaHoraFin] > [FechaHoraInicio]");
        });
        b.HasKey(c => c.Id);

        b.Property(c => c.PacienteNombre).HasMaxLength(150).IsRequired();
        b.Property(c => c.PacienteIdentidad).HasMaxLength(13).IsFixedLength().IsUnicode(false).IsRequired();
        b.Property(c => c.PacienteTelefono).HasMaxLength(20).IsUnicode(false);
        b.Property(c => c.PacienteCorreo).HasMaxLength(150);

        // Hora local de la clínica. datetime2(0): precisión de segundos, sin fracciones
        b.Property(c => c.FechaHoraInicio).HasColumnType("datetime2(0)");
        b.Property(c => c.FechaHoraFin).HasColumnType("datetime2(0)");

        b.Property(c => c.Motivo).HasMaxLength(500).IsRequired();
        b.Property(c => c.Notas).HasMaxLength(2000);
        b.Property(c => c.MotivoCancelacion).HasMaxLength(500);

        b.Property(c => c.Estado).HasConversion<string>().HasMaxLength(20).IsUnicode(false).IsRequired();

        b.Ignore(c => c.DuracionMinutos);

        b.HasIndex(c => new { c.MedicoId, c.FechaHoraInicio });
        b.HasIndex(c => c.PacienteIdentidad);
        b.HasIndex(c => c.Estado);
    }
}
