using Kash.Domain;
using SergioIzq.Application.Kernel.Messaging.Abstracts.Commands;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Inversiones.Commands;

public sealed class UpdateInversionCommandHandler
    : AbsUpdateCommandHandler<Inversion, InversionId, InversionDto, UpdateInversionCommand>
{
    public UpdateInversionCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Inversion, InversionId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    /// <summary>
    /// Valida tipo y verifica propiedad antes de cargar la entidad para actualizar.
    /// </summary>
    protected override async Task<Result> ValidateBeforeUpdateAsync(
        UpdateInversionCommand command,
        CancellationToken cancellationToken)
    {
        if (!TipoInversionConverter.IsValid(command.Tipo))
        {
            return Result.Failure(
                Error.Validation($"Tipo inválido. Valores permitidos: {string.Join(", ", TipoInversionConverter.ValidValues)}."));
        }

        // Verificar que la inversión existe y pertenece al usuario
        var entity = await _writeRepository.GetByIdAsync(command.Id, cancellationToken);

        if (entity is null || entity.UsuarioId.Value != _userContext.UserId)
            return Result.Failure(InversionErrors.NotFound);

        return Result.Success();
    }

    protected override void ApplyChanges(
        Inversion entity,
        UpdateInversionCommand command,
        Dictionary<string, object>? dependencies = null)
    {
        var result = entity.Update(
            command.Nombre,
            command.Ticker,
            command.Tipo,
            command.Cantidad,
            command.PrecioCompra,
            command.Moneda,
            command.FechaCompra,
            command.Descripcion,
            command.Plataforma);

        if (result.IsFailure)
            throw new ArgumentException(result.Error.Message);
    }
}
