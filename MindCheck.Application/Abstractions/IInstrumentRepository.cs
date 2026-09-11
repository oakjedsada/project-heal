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

    /// <summary>
    /// Applies an already-validated edit to an existing instrument's basic
    /// info, questions/choices, and scoring rules, all in one transaction.
    /// <paramref name="questionIdsToDelete"/> and <paramref name="choiceIdsToDelete"/>
    /// (choices removed from a question that itself is being kept) and
    /// <paramref name="scoringRuleIdsToDelete"/> are exactly the rows the
    /// caller has already confirmed are safe to remove — see
    /// UpdateInstrumentUseCase.
    /// </summary>
    Task<Instrument> UpdateFullInstrumentAsync(
        InstrumentId instrumentId,
        string code,
        string name,
        string version,
        string source,
        bool isActive,
        IReadOnlyList<QuestionUpdateSpec> questionSpecs,
        IReadOnlyList<QuestionId> questionIdsToDelete,
        IReadOnlyList<ChoiceId> choiceIdsToDelete,
        IReadOnlyList<ScoringRuleUpdateSpec> scoringRuleSpecs,
        IReadOnlyList<int> scoringRuleIdsToDelete,
        CancellationToken cancellationToken);
}
