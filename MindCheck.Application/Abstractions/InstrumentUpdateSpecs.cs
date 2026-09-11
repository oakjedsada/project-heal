namespace MindCheck.Application.Abstractions;

// Plain specs (not Domain objects, not the wire-facing Dtos) for
// UpdateInstrumentUseCase to hand to IInstrumentRepository.UpdateFullInstrumentAsync
// once every safety check has already passed — building the actual mutated
// Instrument/Question/Choice/ScoringRule graph from these is Infrastructure's
// job, same division of labor as ScoringRuleSpec/RiskRuleSpec for creation.
public sealed record ChoiceUpdateSpec(int? ChoiceId, string Label, int Score, int OrderNo);

public sealed record QuestionUpdateSpec(int? QuestionId, string Text, int OrderNo, IReadOnlyList<ChoiceUpdateSpec> Choices);

public sealed record ScoringRuleUpdateSpec(int? ScoringRuleId, int Min, int Max, string Level, string Interpretation, string Advice);
