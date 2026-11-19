using Citas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Citas.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Medico> Medicos { get; }
    DbSet<Cita> Citas { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
