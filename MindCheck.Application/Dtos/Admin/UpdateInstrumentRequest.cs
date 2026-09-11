namespace MindCheck.Application.Dtos.Admin;

// A null id on a nested item means "new row, create it"; an id matching an
// existing row means "keep and update it"; an existing row whose id is
// absent from the submitted list is treated as "deleted" — see
// UpdateInstrumentUseCase for the safety checks that run before anything is
// actually removed. Risk rules aren't editable here on purpose (out of
// scope for this feature) and are left untouched.
public sealed record UpdateQuestionRequest(int? QuestionId, string Text, int OrderNo, IReadOnlyList<UpdateChoiceRequest> Choices);

public sealed record UpdateChoiceRequest(int? ChoiceId, string Label, int Score, int OrderNo);

public sealed record UpdateScoringRuleRequest(int? ScoringRuleId, int Min, int Max, string Level, string Interpretation, string Advice);

public sealed record UpdateInstrumentRequest(
    string Code,
    string Name,
    string Version,
    string Source,
    bool IsActive,
    IReadOnlyList<UpdateQuestionRequest> Questions,
    IReadOnlyList<UpdateScoringRuleRequest> ScoringRules);
