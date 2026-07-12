using Kash.Domain;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Inversiones;

public class InversionWriteRepository : AbsWriteRepository<Inversion, InversionId>, IInversionWriteRepository
{
    public InversionWriteRepository(KashDbContext context) : base(context)
    {
    }
}
