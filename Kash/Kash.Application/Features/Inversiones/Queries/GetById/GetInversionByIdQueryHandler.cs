using Kash.Domain;
using Kash.Shared.Application.Abstractions.Servicies;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Application.Interfaces;
using Kash.Shared.Domain.Abstractions.Results;
using Kash.Shared.Domain.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;
using MediatR;

namespace Kash.Application.Features.Inversiones.Queries;

public sealed class GetInversionByIdQueryHandler
    : IRequestHandler<GetInversionByIdQuery, Result<InversionDto>>
{
    private readonly IReadRepository<Inversion, InversionDto, InversionId> _readRepository;
    private readonly IWriteRepository<Inversion, InversionId> _writeRepository;
    private readonly IUserContext _userContext;

    public GetInversionByIdQueryHandler(
        IReadRepository<Inversion, InversionDto, InversionId> readRepository,
        IWriteRepository<Inversion, InversionId> writeRepository,
        IUserContext userContext)
    {
        _readRepository  = readRepository;
        _writeRepository = writeRepository;
        _userContext     = userContext;
    }

    public async Task<Result<InversionDto>> Handle(
        GetInversionByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Verificar propiedad usando el write repository (EF Core, incluye UsuarioId)
        var entity = await _writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null || entity.UsuarioId.Value != _userContext.UserId)
            return Result.Failure<InversionDto>(InversionErrors.NotFound);

        // 2. Devolver el DTO desde el read repository (Dapper)
        var dto = await _readRepository.GetReadModelByIdAsync(request.Id, cancellationToken);

        if (dto is null)
            return Result.Failure<InversionDto>(InversionErrors.NotFound);

        return Result.Success(dto);
    }
}
