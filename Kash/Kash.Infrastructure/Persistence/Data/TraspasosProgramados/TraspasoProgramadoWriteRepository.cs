using Kash.Domain;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.TraspasosProgramados
{

    // Nota: Asegúrate de que ITraspasoProgramadoWriteRepository herede de IWriteRepository<TraspasoProgramado>
    public class TraspasoProgramadoWriteRepository : AbsWriteRepository<TraspasoProgramado, TraspasoProgramadoId>, ITraspasoProgramadoWriteRepository
    {
        public TraspasoProgramadoWriteRepository(KashDbContext context) : base(context)
        {
        }
    }
}
