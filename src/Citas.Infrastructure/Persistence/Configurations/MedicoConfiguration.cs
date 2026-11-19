using Citas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Citas.Infrastructure.Persistence.Configurations;

internal sealed class MedicoConfiguration : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> b)
    {
        b.ToTable("Medicos");
        b.HasKey(m => m.Id);

        b.Property(m => m.Nombres).HasMaxLength(100).IsRequired();
        b.Property(m => m.Apellidos).HasMaxLength(100).IsRequired();
        b.Property(m => m.Especialidad).HasMaxLength(100).IsRequired();

        // Solo letras, números y guiones: varchar en vez de nvarchar ocupa la mitad
        b.Property(m => m.NumeroColegiacion).HasMaxLength(20).IsUnicode(false).IsRequired();
        b.Property(m => m.Telefono).HasMaxLength(20).IsUnicode(false);
        b.Property(m => m.Correo).HasMaxLength(150);

        b.HasIndex(m => m.NumeroColegiacion).IsUnique();
        b.HasIndex(m => m.Especialidad);

        b.HasMany(m => m.Citas)
            .WithOne(c => c.Medico)
            .HasForeignKey(c => c.MedicoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Navigation(m => m.Citas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
