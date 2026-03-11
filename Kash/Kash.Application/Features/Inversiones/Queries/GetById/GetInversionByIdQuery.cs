using Kash.Domain;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.Abstractions.Results;
using MediatR;

namespace Kash.Application.Features.Inversiones.Queries;

/// <summary>
/// Obtiene una inversión por ID, validando que pertenezca al usuario autenticado.
/// </summary>
public sealed record GetInversionByIdQuery(Guid Id) : IRequest<Result<InversionDto>>;
