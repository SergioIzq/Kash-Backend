using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;

namespace AhorroLand.Application.Features.GastosProgramados.Commands;

public sealed class UpdateGastoProgramadoCommandHandler
  : AbsUpdateCommandHandler<GastoProgramado, GastoProgramadoDto, UpdateGastoProgramadoCommand>
{
    public UpdateGastoProgramadoCommandHandler(
 IUnitOfWork unitOfWork,
        IWriteRepository<GastoProgramado> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<GastoProgramado, GastoProgramadoDto> readOnlyRepository
      )
        : base(unitOfWork, writeRepository, cacheService)
    {
    }

    protected override void ApplyChanges(GastoProgramado entity, UpdateGastoProgramadoCommand command)
    {
        // Nota: Si la entidad GastoProgramado no tiene un método Update,
        // esta implementación debería agregarse al modelo de dominio.
        throw new NotSupportedException("La actualización de gastos programados requiere implementación en el modelo de dominio.");
    }
}

