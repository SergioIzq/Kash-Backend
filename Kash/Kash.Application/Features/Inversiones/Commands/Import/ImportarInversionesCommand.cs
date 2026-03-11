using Kash.Application.Features.Inversiones.Commands.Import.Models;
using Kash.Shared.Domain.Abstractions.Results;
using MediatR;

namespace Kash.Application.Features.Inversiones.Commands.Import;

public sealed record ImportarInversionesCommand(
    byte[] FileContent,
    string BrokerFormat) : IRequest<Result<ImportarExtractoResult>>;
