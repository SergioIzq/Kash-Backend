using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

namespace AhorroLand.Application.Features.Ingresos.Commands;

/// <summary>
/// Maneja la creación de una nueva entidad Ingreso.
/// </summary>
public sealed class UpdateIngresoCommandHandler
    : AbsUpdateCommandHandler<Ingreso, IngresoId, IngresoDto, UpdateIngresoCommand>
{
    private readonly IDomainValidator _validator;
    public UpdateIngresoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso, IngresoId> writeRepository,
        ICacheService cacheService,
        IReadRepositoryWithDto<Ingreso, IngresoDto, IngresoId> readOnlyRepository,
        IDomainValidator validator
        )
        : base(unitOfWork, writeRepository, cacheService)
    {
        _validator = validator;
    }

    protected override void ApplyChanges(Ingreso entity, UpdateIngresoCommand command)
    {
        var importeVO = Cantidad.Create(command.Importe).Value;
        var fechaVO = FechaRegistro.Create(command.Fecha).Value;
        var conceptoIdVO = ConceptoId.Create(command.ConceptoId).Value;
        var categoriaIdVO = CategoriaId.Create(command.CategoriaId).Value;
        var clienteId = ClienteId.Create(command.ClienteId).Value;
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
            _validator.ExistsAsync < Cliente, ClienteId >(ClienteId.Create(command.ClienteId).Value),
            _validator.ExistsAsync < Persona, PersonaId >(PersonaId.Create(command.PersonaId).Value)
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
