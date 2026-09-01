using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

public interface IInstrumentRepository
{
    Task<Instrument?> GetByIdAsync(InstrumentId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ScoringRule>> GetScoringRulesAsync(InstrumentId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<RiskRule>> GetRiskRulesAsync(InstrumentId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Instrument>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a new instrument (with its Questions/Choices graph) plus the
    /// scoring/risk rules built from <paramref name="scoringRuleSpecs"/> and
    /// <paramref name="riskRuleSpecs"/>, all in one transaction. Rules are
    /// specs rather than Domain objects because they need the instrument's
    /// and questions' database-generated ids, which don't exist until the
    /// instrument itself has been saved once.
    /// </summary>
    Task<Instrument> CreateFullInstrumentAsync(
        Instrument instrumentWithQuestions,
        IReadOnlyList<ScoringRuleSpec> scoringRuleSpecs,
        IReadOnlyList<RiskRuleSpec> riskRuleSpecs,
        CancellationToken cancellationToken);
}
