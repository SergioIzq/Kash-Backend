using AhorroLand.Application.Features.Gastos.Commands;
using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;

public sealed class CreateGastoCommandHandler
    : AbsCreateCommandHandler<Gasto, Guid, CreateGastoCommand>
{
    private readonly IDomainValidator _validator;

    public CreateGastoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Gasto> writeRepository,
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
            _validator.ExistsAsync<Concepto>(command.ConceptoId),
            _validator.ExistsAsync<Categoria>(command.CategoriaId),
            _validator.ExistsAsync<Cuenta>(command.CuentaId),
            _validator.ExistsAsync<FormaPago>(command.FormaPagoId),
            _validator.ExistsAsync<Proveedor>(command.ProveedorId),
            _validator.ExistsAsync<Persona>(command.PersonaId)
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
            var importeVO = new Cantidad(command.Importe);
            var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);
            var fechaVO = new FechaRegistro(command.Fecha);

            // VOs de Identidad y Nombre (Aplanados)
            var conceptoId = new ConceptoId(command.ConceptoId);
            var categoriaId = new CategoriaId(command.CategoriaId);

            var proveedorId = new ProveedorId(command.ProveedorId);
            var personaId = new PersonaId(command.PersonaId);

            var cuentaId = new CuentaId(command.CuentaId);
            var formaPagoId = new FormaPagoId(command.FormaPagoId);

            var usuarioId = new UsuarioId(command.UsuarioId);

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
