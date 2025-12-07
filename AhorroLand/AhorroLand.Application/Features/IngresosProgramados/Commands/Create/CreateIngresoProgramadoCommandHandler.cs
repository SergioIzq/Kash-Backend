using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Application.Interfaces;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.IngresosProgramados.Commands;

public sealed class CreateIngresoProgramadoCommandHandler
    : AbsCreateCommandHandler<IngresoProgramado, IngresoProgramadoId, CreateIngresoProgramadoCommand>
{
    private readonly IJobSchedulingService _jobSchedulingService;

    public CreateIngresoProgramadoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<IngresoProgramado, IngresoProgramadoId> writeRepository,
        ICacheService cacheService,
        IJobSchedulingService jobSchedulingService,
        IUserContext userContext)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _jobSchedulingService = jobSchedulingService;
    }

    protected override IngresoProgramado CreateEntity(CreateIngresoProgramadoCommand command)
    {
        var importeVO = Cantidad.Create(command.Importe).Value;
        var frecuenciaVO = Frecuencia.Create(command.Frecuencia).Value;
        var descripcionVO = new Descripcion(command.Descripcion);
        var conceptoIdVO = ConceptoId.Create(command.ConceptoId).Value;
        var categoriaIdVO = CategoriaId.Create(command.CategoriaId).Value;
        var clienteIdVO = ClienteId.Create(command.ClienteId).Value;
        var personaIdVO = PersonaId.Create(command.PersonaId).Value;
        var cuentaIdVO = CuentaId.Create(command.CuentaId).Value;
        var formaPagoIdVO = FormaPagoId.Create(command.FormaPagoId).Value;

        // Uso del servicio de infraestructura para generar el JobId
        var hangfireJobId = _jobSchedulingService.GenerateJobId();

        var newIngresoProgramado = IngresoProgramado.Create(
            importeVO,
            command.FechaEjecucion!.Value,
            conceptoIdVO,
            clienteIdVO,
            frecuenciaVO,
            personaIdVO,
            cuentaIdVO,
            formaPagoIdVO,
            hangfireJobId,
            descripcionVO
        );

        return newIngresoProgramado;
    }
}

