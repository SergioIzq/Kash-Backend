using Kash.Domain;
using Kash.Shared.Application.Abstractions.Messaging.Abstracts.Queries;
using Kash.Shared.Application.Dtos;
using Kash.Shared.Domain.ValueObjects.Ids;

namespace Kash.Application.Features.ReglasCategorizacion.Queries;

public sealed record GetReglaCategorizacionByIdQuery(Guid Id)
    : AbsGetByIdQuery<ReglaCategorizacion, ReglaCategorizacionId, ReglaCategorizacionDto>(Id);
