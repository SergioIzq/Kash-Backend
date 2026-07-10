using Kash.Domain;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.GastosProgramados
{

    // Nota: Asegúrate de que IGastoProgramadoWriteRepository herede de IWriteRepository<GastoProgramado>
    public class GastoProgramadoWriteRepository : AbsWriteRepository<GastoProgramado, GastoProgramadoId>, IGastoProgramadoWriteRepository
    {
        public GastoProgramadoWriteRepository(KashDbContext context) : base(context)
        {
        }
    }
}
