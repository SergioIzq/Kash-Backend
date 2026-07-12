using Kash.Domain;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Traspasos
{

    // Nota: Asegúrate de que ITraspasoWriteRepository herede de IWriteRepository<Traspaso>
    public class TraspasoWriteRepository : AbsWriteRepository<Traspaso, TraspasoId>, ITraspasoWriteRepository
    {
        public TraspasoWriteRepository(KashDbContext context) : base(context)
        {
        }
    }
}
