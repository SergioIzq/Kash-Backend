using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Domain
{
    public interface ITraspasoProgramadoWriteRepository : IWriteRepository<TraspasoProgramado, TraspasoProgramadoId>
    {
    }
}