using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Gastos.Commands;

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
        IDomainValidator validator,
        IUserContext userContext
        )
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _validator = validator;
    }

    protected override void ApplyChanges(Gasto entity, UpdateGastoCommand command, Dictionary<string, object>? dependencies = null)
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
