using AhorroLand.Infrastructure.Persistence.Interceptors;
using AhorroLand.Shared.Domain.Abstractions;
using AhorroLand.Domain;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AhorroLand.Infrastructure.Persistence.Command;

public class AhorroLandDbContext : DbContext
{
    private readonly DomainEventDispatcherInterceptor _domainEventDispatcher;

    public AhorroLandDbContext(
        DbContextOptions<AhorroLandDbContext> options,
        DomainEventDispatcherInterceptor domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_domainEventDispatcher);

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
      .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(AbsEntity)))
     .ToArray();

// 2. Registrar cada entidad encontrada
        foreach (var type in entityTypes)
        {
            modelBuilder.Entity(type);
        }

        // 3. ✅ FIX CRÍTICO: Aplicar configuraciones desde Infrastructure (no Domain)
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