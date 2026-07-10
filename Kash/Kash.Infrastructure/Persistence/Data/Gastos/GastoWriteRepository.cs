using Kash.Domain;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Gastos
{

    // Nota: Asegúrate de que IGastoWriteRepository herede de IWriteRepository<Gasto>
    public class GastoWriteRepository : AbsWriteRepository<Gasto, GastoId>, IGastoWriteRepository
    {
        public GastoWriteRepository(KashDbContext context) : base(context)
        {
        }
    }
}
