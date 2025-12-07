using AhorroLand.Application.Features.Ingresos.Commands;
using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Dtos;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.Interfaces.Repositories;
using AhorroLand.Shared.Domain.ValueObjects;
using Mapster;
using AhorroLand.Shared.Domain.ValueObjects.Ids;
using AhorroLand.Shared.Application.Interfaces;

public sealed class CreateIngresoCommandHandler
    : AbsCreateCommandHandler<Ingreso, IngresoId, CreateIngresoCommand>
{
    private readonly IDomainValidator _validator;

    public CreateIngresoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso, IngresoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator,
        IUserContext userContext)
    : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _validator = validator;
    }

    public override async Task<Result<Guid>> Handle(
        CreateIngresoCommand command, CancellationToken cancellationToken)
    {
        var validations = new List<(string Entity, Guid Id, Task<bool> Task)>
                        {
                            ("Concepto", command.ConceptoId, _validator.ExistsAsync<Concepto, ConceptoId>(ConceptoId.Create(command.ConceptoId).Value)),
                            ("Categoria", command.CategoriaId, _validator.ExistsAsync<Categoria, CategoriaId>(CategoriaId.Create(command.CategoriaId).Value)),
                            ("Cuenta", command.CuentaId, _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaId).Value)),
                            ("FormaPago", command.FormaPagoId, _validator.ExistsAsync<FormaPago, FormaPagoId>(FormaPagoId.Create(command.FormaPagoId).Value)),
                            ("Cliente", command.ClienteId, _validator.ExistsAsync<Cliente, ClienteId>(ClienteId.Create(command.ClienteId).Value)),
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

            var clienteId = ClienteId.Create(command.ClienteId).Value;
            var personaId = PersonaId.Create(command.PersonaId).Value;

            var cuentaId = CuentaId.Create(command.CuentaId).Value;
            var formaPagoId = FormaPagoId.Create(command.FormaPagoId).Value;

            var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

            // 3. CREACIÓN DE LA ENTIDAD DE DOMINIO (Ingreso)
            var ingreso = Ingreso.Create(
                importeVO,
                fechaVO,
                conceptoId,
                clienteId,
                personaId,
                cuentaId,
                formaPagoId,
                usuarioId,
                descripcionVO);

            // 4. PERSISTENCIA
            _writeRepository.Add(ingreso);
            var entityResult = await base.CreateAsync(ingreso, cancellationToken);

            if (entityResult.IsFailure)
            {
                return Result.Failure<Guid>(entityResult.Error);
            }

            // 5. MAPEO Y ÉXITO
            return Result.Success(entityResult.Value);
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

    protected override Ingreso CreateEntity(CreateIngresoCommand command)
    {
        throw new NotImplementedException("CreateEntity no debe usarse. La lógica de creación asíncrona reside en el método Handle.");
    }
}
