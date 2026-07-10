using Kash.Domain;
using Kash.Infrastructure.Persistence.Command;
using SergioIzq.Infrastructure.Kernel.Persistence;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Infrastructure.Persistence.Data.Usuarios
{

    public class UsuarioWriteRepository : AbsWriteRepository<Usuario, UsuarioId>, IUsuarioWriteRepository
    {
        public UsuarioWriteRepository(KashDbContext context) : base(context)
        {
        }
    }
}
