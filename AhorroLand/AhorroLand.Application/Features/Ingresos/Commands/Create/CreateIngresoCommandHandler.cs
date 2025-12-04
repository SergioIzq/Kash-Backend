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

public sealed class CreateIngresoCommandHandler
    : AbsCreateCommandHandler<Ingreso, IngresoId, CreateIngresoCommand>
{
    private readonly IDomainValidator _validator;

    public CreateIngresoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso, IngresoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator)
    : base(unitOfWork, writeRepository, cacheService)
    {
        _validator = validator;
    }

    public override async Task<Result<Guid>> Handle(
        CreateIngresoCommand command, CancellationToken cancellationToken)
    {
        var existenceTasks = new List<Task<bool>>
        {
            _validator.ExistsAsync<Concepto, ConceptoId>(ConceptoId.Create(command.ConceptoId).Value),
            _validator.ExistsAsync<Categoria, CategoriaId>(CategoriaId.Create(command.CategoriaId).Value),
            _validator.ExistsAsync < Cuenta, CuentaId >(CuentaId.Create(command.CuentaId).Value),
            _validator.ExistsAsync < FormaPago, FormaPagoId >(FormaPagoId.Create(command.FormaPagoId).Value),
            _validator.ExistsAsync < Cliente, ClienteId >(ClienteId.Create(command.ClienteId).Value),
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
