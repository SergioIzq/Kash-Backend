using AhorroLand.Application.Features.Gastos.Commands;
using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Interfaces;
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
        IDomainValidator validator,
        IUserContext userContext)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _validator = validator;
    }

    public override async Task<Result<Guid>> Handle(
        CreateGastoCommand command, CancellationToken cancellationToken)
    {
        var validations = new List<(string Entity, Guid Id, Task<bool> Task)>
                        {
                            ("Concepto", command.ConceptoId, _validator.ExistsAsync<Concepto, ConceptoId>(ConceptoId.Create(command.ConceptoId).Value)),
                            ("Categoria", command.CategoriaId, _validator.ExistsAsync<Categoria, CategoriaId>(CategoriaId.Create(command.CategoriaId).Value)),
                            ("Cuenta", command.CuentaId, _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaId).Value)),
                            ("FormaPago", command.FormaPagoId, _validator.ExistsAsync<FormaPago, FormaPagoId>(FormaPagoId.Create(command.FormaPagoId).Value)),
                            ("Proveedor", command.ProveedorId, _validator.ExistsAsync<Proveedor, ProveedorId>(ProveedorId.Create(command.ProveedorId).Value)),
                            ("Persona", command.PersonaId, _validator.ExistsAsync<Persona, PersonaId>(PersonaId.Create(command.PersonaId).Value))
                        };

        await Task.WhenAll(validations.Select(x => x.Task));

        // 4. Si hay fallos, devolvemos el detalle exacto
        var failedEntities = validations
           .Where(x => !x.Task.Result) // Aquí ya tenemos el resultado
           .Select(x => $"{x.Entity}")
           .ToList();

        // 4. Si hay fallos, devolvemos el detalle exacto
        if (failedEntities.Any())
        {
            var msg = $"No se encontraron las siguientes entidades: {string.Join(", ", failedEntities)}";
            return Result.Failure<Guid>(Error.NotFound(msg));
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
