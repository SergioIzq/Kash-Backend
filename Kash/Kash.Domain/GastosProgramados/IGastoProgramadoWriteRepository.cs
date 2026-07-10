using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain
{
    public interface IGastoProgramadoWriteRepository : IWriteRepository<GastoProgramado, GastoProgramadoId>
    {
    }
}