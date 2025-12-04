using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Gastos.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Gasto.
/// </summary>
public sealed class UpdateGastoCommandHandler
    : AbsUpdateCommandHandler<Gasto, GastoId, GastoDto, UpdateGastoCommand>
{
    private readonly IDomainValidator _validator;
    public UpdateGastoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Gasto, GastoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
        _validator = validator;
    }

    protected override void ApplyChanges(Gasto entity, UpdateGastoCommand command)
    {
        var importeVO = Cantidad.Create(command.Importe).Value;
        var fechaVO = FechaRegistro.Create(command.Fecha).Value;
        var conceptoIdVO = ConceptoId.Create(command.ConceptoId).Value;
        var categoriaIdVO = CategoriaId.Create(command.CategoriaId).Value;
        var proveedorIdVO = ProveedorId.Create(command.ProveedorId).Value;
        var personaIdVO = PersonaId.Create(command.PersonaId).Value;
        var cuentaIdVO = CuentaId.Create(command.CuentaId).Value;
        var formaPagoIdVO = FormaPagoId.Create(command.FormaPagoId).Value;
        var usuarioIdVO = UsuarioId.Create(command.UsuarioId).Value;
        var descripcionVO = new Descripcion(command.Descripcion);


        var existenceTasks = new List<Task<bool>>
        {
            _validator.ExistsAsync < Concepto, ConceptoId >(ConceptoId.Create(command.ConceptoId).Value),
            _validator.ExistsAsync < Categoria, CategoriaId >(CategoriaId.Create(command.CategoriaId).Value),
            _validator.ExistsAsync < Cuenta, CuentaId >(CuentaId.Create(command.CuentaId).Value),
            _validator.ExistsAsync < FormaPago, FormaPagoId >(FormaPagoId.Create(command.FormaPagoId).Value),
            _validator.ExistsAsync < Proveedor, ProveedorId >(ProveedorId.Create(command.ProveedorId).Value),
            _validator.ExistsAsync < Persona, PersonaId >(PersonaId.Create(command.PersonaId).Value)
        };

        entity.Update(
            importeVO,
            fechaVO,
            conceptoIdVO,
            proveedorIdVO,
            personaIdVO,
            cuentaIdVO,
            formaPagoIdVO,
            usuarioIdVO,
            descripcionVO
        );
    }
}
