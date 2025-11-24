using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Ingresos.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Ingreso.
/// </summary>
public sealed class UpdateIngresoCommandHandler
    : AbsUpdateCommandHandler<Ingreso, IngresoDto, UpdateIngresoCommand>
{
    private readonly IDomainValidator _validator;
    public UpdateIngresoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<Ingreso, IngresoDto> readOnlyRepository,
        IDomainValidator validator
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
        _validator = validator;
    }

    protected override void ApplyChanges(Ingreso entity, UpdateIngresoCommand command)
    {
        var importeVO = new Cantidad(command.Importe);
        var fechaVO = new FechaRegistro(command.Fecha);
        var conceptoIdVO = new ConceptoId(command.ConceptoId);
        var categoriaIdVO = new CategoriaId(command.CategoriaId);
        var clienteId = new ClienteId(command.ClienteId);
        var personaIdVO = new PersonaId(command.PersonaId);
        var cuentaIdVO = new CuentaId(command.CuentaId);
        var formaPagoIdVO = new FormaPagoId(command.FormaPagoId);
        var usuarioIdVO = new UsuarioId(command.UsuarioId);
        var descripcionVO = new Descripcion(command.Descripcion);


        var existenceTasks = new List<Task<bool>>
        {
            _validator.ExistsAsync<Concepto>(command.ConceptoId),
            _validator.ExistsAsync<Categoria>(command.CategoriaId),
            _validator.ExistsAsync<Cuenta>(command.CuentaId),
            _validator.ExistsAsync<FormaPago>(command.FormaPagoId),
            _validator.ExistsAsync<Cliente>(command.ClienteId),
            _validator.ExistsAsync<Persona>(command.PersonaId)
        };

        entity.Update(
            importeVO,
            fechaVO,
            conceptoIdVO,
            clienteId,
            personaIdVO,
            cuentaIdVO,
            formaPagoIdVO,
            usuarioIdVO,
            descripcionVO
        );
    }
}
