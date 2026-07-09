using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Commands;

public sealed class CreateReglaCategorizacionCommandHandler
    : AbsCreateCommandHandler<ReglaCategorizacion, ReglaCategorizacionId, CreateReglaCategorizacionCommand>
{
    public CreateReglaCategorizacionCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<ReglaCategorizacion, ReglaCategorizacionId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// HOOK: Crea la entidad de dominio. La validación de campos ocurre dentro de
    /// ReglaCategorizacion.Create, que lanza ArgumentException-compatible Result.Failure
    /// capturado como excepción por la clase base si se usa .Value sobre un Result fallido.
    /// </summary>
    protected override ReglaCategorizacion CreateEntity(
        CreateReglaCategorizacionCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

        var result = ReglaCategorizacion.Create(
            command.Patron,
            command.Tipo,
            command.CategoriaNombre,
            command.ConceptoNombre,
            command.ProveedorNombre,
            command.FormaPagoNombre,
            command.Prioridad,
            command.Activo,
            usuarioId);

        // Propagamos el mensaje de validación real como ArgumentException,
        // que la clase base convierte en Error.Validation (en vez de un error genérico).
        if (result.IsFailure)
        {
            throw new ArgumentException(result.Error.Message);
        }

        return result.Value;
    }
}
