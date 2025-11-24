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

public sealed class CreateIngresoCommandHandler
    : AbsCreateCommandHandler<Ingreso, IngresoDto, CreateIngresoCommand>
{
    private readonly IDomainValidator _validator;

    public CreateIngresoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Ingreso> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator)
    : base(unitOfWork, writeRepository, cacheService)
    {
        _validator = validator;
    }

    public override async Task<Result<IngresoDto>> Handle(
        CreateIngresoCommand command, CancellationToken cancellationToken)
    {
        var existenceTasks = new List<Task<bool>>
        {
            _validator.ExistsAsync<Concepto>(command.ConceptoId),
            _validator.ExistsAsync<Categoria>(command.CategoriaId),
            _validator.ExistsAsync<Cuenta>(command.CuentaId),
            _validator.ExistsAsync<FormaPago>(command.FormaPagoId),
            _validator.ExistsAsync<Cliente>(command.ClienteId),
            _validator.ExistsAsync<Persona>(command.PersonaId)
        };

        var results = await Task.WhenAll(existenceTasks);

        if (results.Any(r => !r))
        {
            return Result.Failure<IngresoDto>(
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

            var clienteId = new ClienteId(command.ClienteId);
            var personaId = new PersonaId(command.PersonaId);

            var cuentaId = new CuentaId(command.CuentaId);
            var formaPagoId = new FormaPagoId(command.FormaPagoId);

            var usuarioId = new UsuarioId(command.UsuarioId);

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
                return Result.Failure<IngresoDto>(entityResult.Error);
            }

            // 5. MAPEO Y ÉXITO
            var dto = entityResult.Value.Adapt<IngresoDto>();
            return Result.Success(dto);
        }
        catch (ArgumentException ex)
        {
            // Captura de errores de validación de Value Objects
            return Result.Failure<IngresoDto>(Error.Validation(ex.Message));
        }
        catch (Exception ex)
        {
            return Result.Failure<IngresoDto>(Error.Failure("Error.Unexpected", "Error Inesperado", ex.Message));
        }
    }

    protected override Ingreso CreateEntity(CreateIngresoCommand command)
    {
        throw new NotImplementedException("CreateEntity no debe usarse. La lógica de creación asíncrona reside en el método Handle.");
    }
}
