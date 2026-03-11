using Kash.Application.Features.Inversiones.Commands;
using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Inversiones.Commands;

public sealed class CreateInversionCommandHandler
    : AbsCreateCommandHandler<Inversion, InversionId, CreateInversionCommand>
{
    public CreateInversionCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Inversion, InversionId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// Validaciones de negocio antes de crear (reglas que no dependen del dominio puro).
    /// </summary>
    protected override Task<Result> ValidateBeforeCreateAsync(
        CreateInversionCommand command,
        CancellationToken cancellationToken)
    {
        if (!TipoInversionConverter.IsValid(command.Tipo))
        {
            return Task.FromResult(Result.Failure(
                Error.Validation($"Tipo inválido. Valores permitidos: {string.Join(", ", TipoInversionConverter.ValidValues)}.")));
        }

        return Task.FromResult(Result.Success());
    }

    protected override Inversion CreateEntity(
        CreateInversionCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        var usuarioId = UsuarioId.Create(_userContext.UserId!.Value).Value;

        var result = Inversion.Create(
            command.Nombre,
            command.Ticker,
            command.Tipo,
            command.Cantidad,
            command.PrecioCompra,
            command.Moneda,
            command.FechaCompra,
            usuarioId,
            command.Descripcion,
            command.Plataforma);

        if (result.IsFailure)
            throw new ArgumentException(result.Error.Message);

        return result.Value;
    }
}
