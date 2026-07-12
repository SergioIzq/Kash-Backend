using Kash.Domain;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.ReglasCategorizacion
{
    public class ReglaCategorizacionWriteRepository
        : AbsWriteRepository<ReglaCategorizacion, ReglaCategorizacionId>, IReglaCategorizacionWriteRepository
    {
        public ReglaCategorizacionWriteRepository(KashDbContext context) : base(context)
        {
        }
    }
}
