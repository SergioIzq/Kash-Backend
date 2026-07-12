using System.Reflection;
using Kash.Domain;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kash.Infrastructure.Persistence.Command;

/// <summary>
/// DbContext de Kash. Toda la mecánica (registro de entidades por convención, configuraciones
/// del assembly, strip de _domainEvents, índice único en Id, SaveChanges optimizado) vive en
/// <see cref="KernelDbContext"/> — aquí solo se indica dónde están las entidades de dominio.
/// </summary>
public class KashDbContext : KernelDbContext
{
    public KashDbContext(DbContextOptions<KashDbContext> options)
        : base(options)
    {
    }

    protected override Assembly DomainAssembly => typeof(Gasto).Assembly;
}
