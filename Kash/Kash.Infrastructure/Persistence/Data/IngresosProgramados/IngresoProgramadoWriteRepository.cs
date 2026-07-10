using Kash.Domain;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.IngresosProgramados
{

    // Nota: Asegúrate de que IIngresoProgramadoWriteRepository herede de IWriteRepository<IngresoProgramado>
    public class IngresoProgramadoWriteRepository : AbsWriteRepository<IngresoProgramado, IngresoProgramadoId>, IIngresoProgramadoWriteRepository
    {
        public IngresoProgramadoWriteRepository(KashDbContext context) : base(context)
        {
        }
    }
}
