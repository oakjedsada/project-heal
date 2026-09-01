using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

public sealed class GetInstrumentDetailUseCase
{
    private readonly IInstrumentRepository _instrumentRepository;

    public GetInstrumentDetailUseCase(IInstrumentRepository instrumentRepository)
    {
        _instrumentRepository = instrumentRepository;
    }

    public async Task<InstrumentDetailDto> ExecuteAsync(InstrumentId instrumentId, CancellationToken cancellationToken)
    {
        var instrument = await _instrumentRepository.GetByIdAsync(instrumentId, cancellationToken)
            ?? throw new InstrumentNotFoundException(instrumentId);

        var questions = instrument.Questions
            .OrderBy(q => q.OrderNo)
            .Select(q => new InstrumentDetailQuestionDto(q.Id.Value, q.Text, q.OrderNo))
            .ToList();

        return new InstrumentDetailDto(instrument.Id.Value, instrument.Code, instrument.Name, questions);
    }
}
