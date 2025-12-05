using AhorroLand.Domain;
using AhorroLand.Shared.Application.Abstractions.Messaging;
using AhorroLand.Shared.Application.Abstractions.Servicies;
using AhorroLand.Shared.Application.Servicies;
using AhorroLand.Shared.Domain.Abstractions.Results;
using AhorroLand.Shared.Domain.Helpers;
using AhorroLand.Shared.Domain.Interfaces;
using AhorroLand.Shared.Domain.ValueObjects;

namespace AhorroLand.Application.Features.Auth.Commands.UploadAvatar;

public sealed class UploadAvatarCommandHandler : ICommandHandler<UploadAvatarCommand, string>
{
    private readonly IUsuarioWriteRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ICacheService _cacheService;

    // Nombre de la carpeta donde se guardarán las imágenes
    private const string ContainerName = "avatars";

    public UploadAvatarCommandHandler(
        IUsuarioWriteRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        ICacheService cacheService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _cacheService = cacheService;
    }

    public async Task<Result<string>> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener el usuario (con tracking)
        var usuario = await _usuarioRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (usuario is null)
        {
            return Result.Failure<string>(Error.NotFound("Usuario no encontrado."));
        }

        // 2. Limpieza: Si ya tiene un avatar, borramos el archivo antiguo del disco
        // Esto evita llenar el servidor de imágenes huerfanas.
        if (!string.IsNullOrEmpty(usuario.Avatar?.Value))
        {
            await _fileStorage.DeleteFileAsync(usuario.Avatar.Value.Value, ContainerName);
        }

        // 3. Guardar el nuevo archivo
        // El servicio se encarga de generar nombre único, crear carpeta y devolver URL.
        var avatarUrlString = await _fileStorage.SaveFileAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            ContainerName
        );

        // 4. Crear el Value Object y Validar
        // Usamos .Create() porque es un dato nuevo que debe cumplir reglas de negocio.
        var avatarResult = AvatarUrl.Create(avatarUrlString);

        if (avatarResult.IsFailure)
        {
            // Si falla (ej. URL demasiado larga), intentamos limpiar el archivo recién subido
            await _fileStorage.DeleteFileAsync(avatarUrlString, ContainerName);
            return Result.Failure<string>(avatarResult.Error);
        }

        // 5. Actualizar la entidad
        usuario.UpdateAvatar(avatarResult.Value);

        // 6. Persistir en Base de Datos
        _usuarioRepository.Update(usuario);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Invalidar Caché
        // Usamos el helper que creamos para asegurar consistencia en la clave
        var cacheKey = CacheKeys.Usuario(request.UserId);
        await _cacheService.RemoveAsync(cacheKey);

        // 8. Retornar la nueva URL
        return Result.Success(avatarUrlString);
    }
}