using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;

namespace MindCheck.Application.UseCases.Admin;

public sealed class ListInstrumentsUseCase
{
    private readonly IInstrumentRepository _instrumentRepository;

    public ListInstrumentsUseCase(IInstrumentRepository instrumentRepository)
    {
        _instrumentRepository = instrumentRepository;
    }

    public async Task<IReadOnlyList<InstrumentSummaryDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var instruments = await _instrumentRepository.GetAllAsync(cancellationToken);
        return instruments
            .OrderBy(i => i.Id.Value)
            .Select(i => new InstrumentSummaryDto(i.Id.Value, i.Code, i.Name))
            .ToList();
    }
}
