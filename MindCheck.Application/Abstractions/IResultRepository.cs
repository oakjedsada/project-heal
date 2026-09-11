using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

public sealed record InstrumentLevelCount(InstrumentId InstrumentId, string Level, int Count);

public interface IResultRepository
{
    Task AddAsync(Result result, CancellationToken cancellationToken);

    Task<IReadOnlyList<Result>> GetBySessionIdAsync(SessionId sessionId, CancellationToken cancellationToken);

    /// <summary>Anonymous aggregate: how many results landed at each level, per instrument.</summary>
    Task<IReadOnlyList<InstrumentLevelCount>> GetLevelCountsAsync(CancellationToken cancellationToken);

    /// <summary>Distinct Level values this instrument already has real Results recorded under — used to block deleting/renaming a scoring rule's level out from under existing history.</summary>
    Task<IReadOnlySet<string>> GetLevelsInUseAsync(InstrumentId instrumentId, CancellationToken cancellationToken);
}
