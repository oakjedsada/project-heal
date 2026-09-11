namespace MindCheck.Application.Dtos.Admin;

// Everything the "edit instrument" admin page needs to prefill its form —
// unlike InstrumentDetailDto (which only carries what the flow-transitions
// question picker needs), this includes choices and scoring rules too.
public sealed record FullChoiceDto(int ChoiceId, string Label, int Score, int OrderNo);

public sealed record FullQuestionDto(int QuestionId, string Text, int OrderNo, IReadOnlyList<FullChoiceDto> Choices);

public sealed record FullScoringRuleDto(int ScoringRuleId, int Min, int Max, string Level, string Interpretation, string Advice);

public sealed record InstrumentFullDetailDto(
    int InstrumentId,
    string Code,
    string Name,
    string Version,
    string Source,
    bool IsActive,
    IReadOnlyList<FullQuestionDto> Questions,
    IReadOnlyList<FullScoringRuleDto> ScoringRules);
