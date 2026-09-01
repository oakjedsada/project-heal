namespace MindCheck.Application.Dtos.Admin;

public sealed record CreateChoiceRequest(string Label, int Score, int OrderNo);

public sealed record CreateQuestionRequest(string Text, int OrderNo, IReadOnlyList<CreateChoiceRequest> Choices);

public sealed record CreateScoringRuleRequest(int Min, int Max, string Level, string Interpretation, string Advice);

// QuestionOrderNo refers to the OrderNo of one of the Questions in this same
// request — the question doesn't have a real id yet at request time.
public sealed record CreateRiskRuleRequest(int QuestionOrderNo, string Operator, int Threshold, string Action);

public sealed record CreateInstrumentRequest(
    string Code,
    string Name,
    string Version,
    string Source,
    bool IsActive,
    IReadOnlyList<CreateQuestionRequest> Questions,
    IReadOnlyList<CreateScoringRuleRequest> ScoringRules,
    IReadOnlyList<CreateRiskRuleRequest> RiskRules);
