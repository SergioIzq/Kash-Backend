using Kash.Application.Features.Inversiones.Commands.Import.Models;
using Kash.Application.Features.Inversiones.Commands.Import.Parsers;
using Kash.Application.Interfaces.Repositories;
using Kash.Domain;
using SergioIzq.Application.Kernel.Services;
using Kash.Shared.Application.Interfaces;
using SergioIzq.Application.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Abstractions.Results;
using SergioIzq.Domain.Kernel.Interfaces;
using SergioIzq.Domain.Kernel.Interfaces.Repositories;
using Kash.Shared.Domain.ValueObjects.Ids;
using MediatR;
using IInversionReadRepository = Kash.Application.Interfaces.Repositories.IInversionReadRepository;

namespace Kash.Application.Features.Inversiones.Commands.Import;

public sealed class ImportarInversionesCommandHandler
    : IRequestHandler<ImportarInversionesCommand, Result<ImportarExtractoResult>>
{
    private readonly IWriteRepository<Inversion, InversionId> _writeRepository;
    private readonly IInversionReadRepository _readRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;
    private readonly GenericCsvParser _genericParser;
    private readonly TradeRepublicCsvParser _tradeRepublicCsvParser;
    private readonly TradeRepublicPdfParser _tradeRepublicPdfParser;
    private readonly DegiroCsvParser _degiroParser;
    private readonly InteractiveBrokersCsvParser _ibkrParser;
    private readonly BinanceCsvParser _binanceParser;

    public ImportarInversionesCommandHandler(
        IWriteRepository<Inversion, InversionId> writeRepository,
        IInversionReadRepository readRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext,
        GenericCsvParser genericParser,
        TradeRepublicCsvParser tradeRepublicCsvParser,
        TradeRepublicPdfParser tradeRepublicPdfParser,
        DegiroCsvParser degiroParser,
        InteractiveBrokersCsvParser ibkrParser,
        BinanceCsvParser binanceParser)
    {
        _writeRepository         = writeRepository;
        _readRepository          = readRepository;
        _unitOfWork              = unitOfWork;
        _userContext             = userContext;
        _genericParser           = genericParser;
        _tradeRepublicCsvParser  = tradeRepublicCsvParser;
        _tradeRepublicPdfParser  = tradeRepublicPdfParser;
        _degiroParser            = degiroParser;
        _ibkrParser              = ibkrParser;
        _binanceParser           = binanceParser;
    }

    public async Task<Result<ImportarExtractoResult>> Handle(
        ImportarInversionesCommand request,
        CancellationToken cancellationToken)
    {
        var usuarioId = _userContext.UserId;
        if (usuarioId is null)
            return Result.Failure<ImportarExtractoResult>(Error.Unauthorized("Usuario no autenticado."));

        IInversionParser parser = request.BrokerFormat switch
        {
            "generic"             => _genericParser,
            "trade_republic"      => _tradeRepublicCsvParser,
            "trade_republic_pdf"  => _tradeRepublicPdfParser,
            "degiro"              => _degiroParser,
            "interactive_brokers" => _ibkrParser,
            "binance"             => _binanceParser,
            _ => null!
        };

        if (parser is null)
            return Result.Failure<ImportarExtractoResult>(
                Error.Validation($"Formato de broker no soportado: {request.BrokerFormat}"));

        var parseResult = await parser.ParseAsync(request.FileContent, cancellationToken);

        var importadas = 0;
        var duplicadas = 0;

        foreach (var dto in parseResult.Rows)
        {
            var isDuplicate = await _readRepository.ExistsDuplicateAsync(
                usuarioId.Value, dto.Ticker, dto.FechaCompra, dto.Cantidad, dto.PrecioCompra, cancellationToken);

            if (isDuplicate)
            {
                duplicadas++;
                continue;
            }

            var usuarioIdVo = UsuarioId.Create(usuarioId.Value).Value;
            var entityResult = Inversion.Create(
                dto.Nombre, dto.Ticker, dto.Tipo, dto.Cantidad, dto.PrecioCompra,
                dto.Moneda, dto.FechaCompra.ToDateTime(TimeOnly.MinValue),
                usuarioIdVo, dto.Descripcion, dto.Plataforma);

            if (entityResult.IsFailure)
            {
                parseResult.Errores.Add(new ImportErrorLinea(0, dto.Ticker, entityResult.Error.Message));
                continue;
            }

            _writeRepository.Add(entityResult.Value);
            importadas++;
        }

        if (importadas > 0)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ImportarExtractoResult(importadas, duplicadas, parseResult.Errores));
    }
}
