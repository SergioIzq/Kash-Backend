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

namespace AhorroLand.Application.Features.Conceptos.Commands;

public sealed class CreateConceptoCommandHandler
    : AbsCreateCommandHandler<Concepto, ConceptoId, CreateConceptoCommand>
{
    // ? Inyectamos IDomainValidator para las consultas rápidas
    private readonly IDomainValidator _validator;

    public CreateConceptoCommandHandler(
        IUnitOfWork unitOfWork,
        IWriteRepository<Concepto, ConceptoId> writeRepository,
        ICacheService cacheService,
        IDomainValidator validator) // Recibimos el validador
    : base(unitOfWork, writeRepository, cacheService)
    {
        _validator = validator;
    }

    // ? CAMBIO CLAVE: Mover la lógica a Handle para hacerlo ASÍNCRONO y eficiente.
    public override async Task<Result<Guid>> Handle(
        CreateConceptoCommand command, CancellationToken cancellationToken)
    {
        // 1. VALIDACIÓN ASÍNCRONA DE EXISTENCIA (SELECT 1)
        var categoriaExists = await _validator.ExistsAsync<Categoria, CategoriaId>(CategoriaId.Create(command.CategoriaId).Value);

        if (!categoriaExists)
        {
            // Devolvemos un Result.Failure si la referencia no es válida.
            return Result.Failure<Guid>(
                Error.NotFound($"Categoria con id {command.CategoriaId} no fue encontrada."));
        }

        // 2. CREACIÓN DE VALUE OBJECTS (VOs)
        try
        {
            var nombreVO = Nombre.Create(command.Nombre).Value;
            var usuarioId = UsuarioId.Create(command.UsuarioId).Value;

            // Creamos el VO de Identidad para la referencia
            var categoriaId = CategoriaId.Create(command.CategoriaId).Value;

            // 3. CREACIÓN DE LA ENTIDAD DE DOMINIO
            // ? NOTA: Concepto.Create debe aceptar CategoriaId en lugar de la entidad Categoria
            var newConcepto = Concepto.Create(
                nombreVO,
                categoriaId, // Usamos el ID validado
                usuarioId);

            var entityResult = await CreateAsync(newConcepto, cancellationToken);

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

    // ? OPTIMIZACIÓN: Lanzar excepción para asegurar que la abstracción base síncrona no se use.
    protected override Concepto CreateEntity(CreateConceptoCommand command)
    {
        throw new NotImplementedException("CreateEntity no debe usarse. La lógica de creación asíncrona reside en el método Handle.");
    }
}
