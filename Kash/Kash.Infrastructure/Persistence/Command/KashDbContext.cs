using Kash.Domain;
using SergioIzq.Domain.Kernel.Abstractions;
using SergioIzq.Domain.Kernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Kash.Infrastructure.Persistence.Command;

public class KashDbContext : DbContext
{
    // El interceptor de eventos de dominio se registra una única vez, en el
    // AddDbContext<KashDbContext>(...) de DependencyInjection.cs. Antes también
    // se inyectaba aquí y se añadía otra vez en OnConfiguring -> el mismo interceptor
    // quedaba registrado dos veces y sus hooks se disparaban dos veces por guardado.
    public KashDbContext(DbContextOptions<KashDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableDetailedErrors();
#endif

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Obtener el assembly de Domain para registrar las entidades
        var domainAssembly = Assembly.GetAssembly(typeof(Gasto));

        if (domainAssembly == null)
        {
            throw new InvalidOperationException("El assembly de Dominio no se pudo cargar.");
        }

        var entityTypes = domainAssembly.GetTypes()
      .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AbsEntity<IGuidValueObject>)))
     .ToArray();

        // 2. Registrar cada entidad encontrada
        foreach (var type in entityTypes)
        {
            modelBuilder.Entity(type);
        }

        // 3. FIX CRÍTICO: Aplicar configuraciones desde Infrastructure (no Domain)
        // Las configuraciones (IEntityTypeConfiguration) están en Infrastructure.Persistence.Command.Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // 4. Configuración adicional para todas las entidades
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Ignorar DomainEvents de todas las entidades
            var domainEventsProperty = entityType.FindProperty("_domainEvents");
            if (domainEventsProperty != null)
            {
                entityType.RemoveProperty(domainEventsProperty);
            }

            // Configurar índices por defecto en Id
            var idProperty = entityType.FindProperty("Id");
            if (idProperty != null)
            {
                var existingIndex = entityType.GetIndexes()
                   .FirstOrDefault(i => i.Properties.Any(p => p.Name == "Id"));

                if (existingIndex == null)
                {
                    modelBuilder.Entity(entityType.ClrType)
           .HasIndex("Id")
                .IsUnique();
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Optimizar detección de cambios
        ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            // Detectar cambios manualmente una sola vez
            ChangeTracker.DetectChanges();
            return await base.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }
}
