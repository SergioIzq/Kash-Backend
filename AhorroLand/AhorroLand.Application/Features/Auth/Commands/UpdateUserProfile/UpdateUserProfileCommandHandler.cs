namespace AhorroLand.Application.Features.Auth.Commands.UpdateUserProfile;

using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Abstractions.Servicies; // Para ICacheService
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;

public sealed class UpdateUserProfileCommandHandler : ICommandHandler<UpdateUserProfileCommand> // Asume que hereda ICommand<Result>
{
    private readonly IUsuarioWriteRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService; // 1. Agregado campo

    // 2. Inyección en constructor
    public UpdateUserProfileCommandHandler(
        IUsuarioWriteRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener usuario
        var usuario = await _usuarioRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (usuario is null)
        {
            return Result.Failure(Error.NotFound("Usuario no encontrado"));
        }

        // 2. Crear Value Objects (Validación de Dominio)
        // Usamos el Factory Method 'Create' para asegurar que los datos sean válidos.
        // Si hay error (ej. nombre vacío), devolvemos el error inmediatamente.

        var nombreResult = Nombre.Create(request.Nombre).Value;
        // Asumimos que si apellidos es null, enviamos string.Empty y el VO lo maneja
        var apellidoResult = Apellido.Create(request.Apellidos ?? string.Empty).Value;

        // 3. Actualizar la entidad
        usuario.Update(nombreResult, apellidoResult);

        // 4. Persistir cambios
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}