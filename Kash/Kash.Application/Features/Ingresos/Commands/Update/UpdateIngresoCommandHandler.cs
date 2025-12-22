using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Commands;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.Ingresos.Commands;

/// <summary>
/// Maneja la actualización de una entidad Ingreso existente.
/// 🔥 OPTIMIZADO: Cliente y Persona son opcionales, validaciones en paralelo.
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
        IDomainValidator validator,
        IUserContext userContext)
        : base(unitOfWork, writeRepository, cacheService, userContext)
    {
        _validator = validator;
    }

    protected override void ApplyChanges(Ingreso entity, UpdateIngresoCommand command)
    {
        // 1. 🚀 VALIDAR ENTIDADES RELACIONADAS EN PARALELO (antes de crear VOs)
        var validations = new List<(string Entity, Guid Id, Task<bool> Task)>
        {
            ("Concepto", command.ConceptoId, _validator.ExistsAsync<Concepto, ConceptoId>(ConceptoId.Create(command.ConceptoId).Value)),
            ("Categoria", command.CategoriaId, _validator.ExistsAsync<Categoria, CategoriaId>(CategoriaId.Create(command.CategoriaId).Value)),
            ("Cuenta", command.CuentaId, _validator.ExistsAsync<Cuenta, CuentaId>(CuentaId.Create(command.CuentaId).Value)),
            ("FormaPago", command.FormaPagoId, _validator.ExistsAsync<FormaPago, FormaPagoId>(FormaPagoId.Create(command.FormaPagoId).Value))
        };

        // 2. 🔥 Validar Cliente y Persona solo si se proporcionan
        if (command.ClienteId.HasValue)
        {
            validations.Add(("Cliente", command.ClienteId.Value, 
                _validator.ExistsAsync<Cliente, ClienteId>(ClienteId.Create(command.ClienteId.Value).Value)));
        }

        if (command.PersonaId.HasValue)
        {
            validations.Add(("Persona", command.PersonaId.Value, 
                _validator.ExistsAsync<Persona, PersonaId>(PersonaId.Create(command.PersonaId.Value).Value)));
        }

        // 3. ⚡ Esperar todas las validaciones (esto es síncrono en ApplyChanges, idealmente debería ser async)
        // NOTA: ApplyChanges es síncrono, por lo que hacemos .Result (no ideal pero es la limitación de la clase base)
        Task.WhenAll(validations.Select(x => x.Task)).Wait();

        // 4. 🔍 Verificar si hay entidades no encontradas
        var failedEntities = validations
            .Where(x => !x.Task.Result)
            .Select(x => x.Entity)
            .ToList();

        if (failedEntities.Any())
        {
            var msg = $"No se encontraron las siguientes entidades: {string.Join(", ", failedEntities)}";
            throw new ArgumentException(msg); // Se captura como Error.Validation en la clase base
        }

        // 5. 🏗️ CONSTRUCCIÓN DE VALUE OBJECTS OBLIGATORIOS
        var importeVO = Cantidad.Create(command.Importe).Value;
        var fechaVO = FechaRegistro.Create(command.Fecha).Value;
        var conceptoIdVO = ConceptoId.Create(command.ConceptoId).Value;
        var categoriaIdVO = CategoriaId.Create(command.CategoriaId).Value;
        var cuentaIdVO = CuentaId.Create(command.CuentaId).Value;
        var formaPagoIdVO = FormaPagoId.Create(command.FormaPagoId).Value;
        var usuarioIdVO = UsuarioId.Create(command.UsuarioId).Value;
        var descripcionVO = new Descripcion(command.Descripcion ?? string.Empty);

        // 6. 🔥 VALUE OBJECTS OPCIONALES - Tipo explícito nullable
        ClienteId? clienteIdVO = command.ClienteId.HasValue 
            ? ClienteId.Create(command.ClienteId.Value).Value 
            : null;

        PersonaId? personaIdVO = command.PersonaId.HasValue 
            ? PersonaId.Create(command.PersonaId.Value).Value 
            : null;

        // 7. 🎯 APLICAR CAMBIOS A LA ENTIDAD
        entity.Update(
            importeVO,
            fechaVO,
            conceptoIdVO,
            clienteIdVO,    // Ahora puede ser null
            personaIdVO,    // Ahora puede ser null
            cuentaIdVO,
            formaPagoIdVO,
            usuarioIdVO,
            descripcionVO
        );
    }
}
