using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.GastosProgramados.Commands;

public sealed class UpdateGastoProgramadoCommandHandler
    : AbsUpdateCommandHandler<GastoProgramado, GastoProgramadoId, GastoProgramadoDto, UpdateGastoProgramadoCommand>
{
    public UpdateGastoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<GastoProgramado, GastoProgramadoId> writeRepository,
        ICacheService cacheService,
        IUserContext userContext
        )
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
    }

    protected override void ApplyChanges(GastoProgramado entity, UpdateGastoProgramadoCommand command)
    {
        // Convertir los datos del comando a Value Objects
        var importeVO = Cantidad.Create(command.Importe).Value;
        var frecuenciaVO = Frecuencia.Create(command.Frecuencia).Value;
        var descripcionVO = string.IsNullOrWhiteSpace(command.Descripcion) 
            ? (Descripcion?)null 
            : new Descripcion(command.Descripcion);
        
        var conceptoIdVO = ConceptoId.Create(command.ConceptoId).Value;
        var proveedorIdVO = ProveedorId.Create(command.ProveedorId).Value;
        var personaIdVO = PersonaId.Create(command.PersonaId).Value;
        var cuentaIdVO = CuentaId.Create(command.CuentaId).Value;
        var formaPagoIdVO = FormaPagoId.Create(command.FormaPagoId).Value;

        // Llamar al método Update de la entidad
        var result = entity.Update(
            importeVO,
            command.FechaEjecucion!.Value,
            conceptoIdVO,
            proveedorIdVO,
            personaIdVO,
            cuentaIdVO,
            formaPagoIdVO,
            frecuenciaVO,
            command.Activo,
            descripcionVO
        );

        // Si el resultado es un fallo, lanzar excepción
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }
    }
}

