using AhorroLand.Application.Features.Gastos.Commands;
using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using AhorroLand.Shared.Domain.ValueObjects.Ids;

public sealed class CreateGastoCommandHandler
    : AbsCreateCommandHandler<Gasto, GastoId, CreateGastoCommand>
{
    private readonly IDomainValidator _validator;

    public CreateGastoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Gasto, GastoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator)
    : base(unitOfWork, writeRepository, cacheService)
    {
        _validator = validator;
    }

    public override async Task<Result<Guid>> Handle(
        CreateGastoCommand command, CancellationToken cancellationToken)
    {
        var existenceTasks = new List<Task<bool>>
        {
            _validator.ExistsAsync<Concepto, ConceptoId>(ConceptoId.Create(command.ConceptoId).Value),
            _validator.ExistsAsync<Categoria, CategoriaId>(CategoriaId.Create(command.CategoriaId).Value),
            _validator.ExistsAsync < Cuenta, CuentaId >(CuentaId.Create(command.CuentaId).Value),
            _validator.ExistsAsync < FormaPago, FormaPagoId >(FormaPagoId.Create(command.FormaPagoId).Value),
            _validator.ExistsAsync < Proveedor, ProveedorId >(ProveedorId.Create(command.ProveedorId).Value),
            _validator.ExistsAsync < Persona, PersonaId >(PersonaId.Create(command.PersonaId).Value)
        };

        var results = await Task.WhenAll(existenceTasks);

        if (results.Any(r => !r))
        {
            return Result.Failure<Guid>(
                Error.NotFound("Una o más entidades referenciadas no existen o el ID es incorrecto."));
        }

        try
        {
            // VOs de Valor
            var importeVO = Cantidad.Create(command.Importe).Value;
            var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
            var fechaVO = FechaRegistro.Create(command.Fecha).Value;

            // VOs de Identidad y Nombre (Aplanados)
            var conceptoId = ConceptoId.Create(command.ConceptoId).Value;
            var categoriaId = CategoriaId.Create(command.CategoriaId).Value;

            var proveedorId = ProveedorId.Create(command.ProveedorId).Value;
            var personaId = PersonaId.Create(command.PersonaId).Value;

            var cuentaId = CuentaId.Create(command.CuentaId).Value;
            var formaPagoId = FormaPagoId.Create(command.FormaPagoId).Value;

            var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

            // 3. CREACIÓN DE LA ENTIDAD DE DOMINIO (Gasto)
            var gasto = Gasto.Create(
                importeVO,
                fechaVO,
                conceptoId,
                proveedorId,
                personaId,
                cuentaId,
                formaPagoId,
                usuarioId,
                descripcionVO);

            var entityResultGuid = await CreateAsync(gasto, cancellationToken);

            if (entityResultGuid.IsFailure)
            {
                return Result.Failure<Guid>(entityResultGuid.Error);
            }

            return Result.Success(entityResultGuid.Value);
        }
        catch (ArgumentException ex)
        {
            // Captura de errores de validación de Value Objects
            return Result.Failure<Guid>(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Failure("Error.Unexpected", "Error Inesperado", ex.Message));
        }
    }

    protected override Gasto CreateEntity(CreateGastoCommand command)
    {
        throw new NotImplementedException("CreateEntity no debe usarse. La lógica de creación asíncrona reside en el método Handle.");
    }
}
