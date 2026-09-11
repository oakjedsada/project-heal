using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

// Everything here is validated *before* UpdateFullInstrumentAsync is called —
// no partial edits. The two real dangers are: deleting a question/choice a
// real user already answered (GetAnsweredQuestionIdsAsync/GetAnsweredChoiceIdsAsync
// would leave dangling Response rows, and could 500 an in-progress session's
// scoring — see SubmitAnswerUseCase's live Choice.Score lookup), and
// removing/renaming a scoring rule's Level string that an existing Result
// already recorded (GetResultUseCase looks up interpretation/advice by
// matching Level live, not from a snapshot — an orphaned Level silently
// blanks both for anyone revisiting that old result).
public sealed class UpdateInstrumentUseCase
{
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly IResponseRepository _responseRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IFlowTransitionRepository _flowTransitionRepository;
    private readonly GetInstrumentFullDetailUseCase _getInstrumentFullDetailUseCase;

    public UpdateInstrumentUseCase(
        IInstrumentRepository instrumentRepository,
        IResponseRepository responseRepository,
        IResultRepository resultRepository,
        IFlowTransitionRepository flowTransitionRepository,
        GetInstrumentFullDetailUseCase getInstrumentFullDetailUseCase)
    {
        _instrumentRepository = instrumentRepository;
        _responseRepository = responseRepository;
        _resultRepository = resultRepository;
        _flowTransitionRepository = flowTransitionRepository;
        _getInstrumentFullDetailUseCase = getInstrumentFullDetailUseCase;
    }

    public async Task<InstrumentFullDetailDto> ExecuteAsync(
        InstrumentId instrumentId,
        UpdateInstrumentRequest request,
        CancellationToken cancellationToken)
    {
        var existing = await _instrumentRepository.GetByIdAsync(instrumentId, cancellationToken)
            ?? throw new InstrumentNotFoundException(instrumentId);

        var code = request.Code?.Trim() ?? string.Empty;
        if (code.Length == 0)
        {
            throw new InvalidAdminRequestException("Code is required.");
        }

        if (!string.Equals(code, existing.Code, StringComparison.Ordinal)
            && await _instrumentRepository.CodeExistsAsync(code, cancellationToken))
        {
            throw new DuplicateInstrumentCodeException(code);
        }

        if (string.IsNullOrWhiteSpace(request.Name)
            || string.IsNullOrWhiteSpace(request.Version)
            || string.IsNullOrWhiteSpace(request.Source))
        {
            throw new InvalidAdminRequestException("Name, version, and source are all required.");
        }

        if (request.Questions.Count == 0)
        {
            throw new InvalidAdminRequestException("An instrument must have at least one question.");
        }

        if (request.ScoringRules.Count == 0)
        {
            throw new InvalidAdminRequestException("An instrument must have at least one scoring rule.");
        }

        var orderNosSeen = new HashSet<int>();
        foreach (var q in request.Questions)
        {
            if (!orderNosSeen.Add(q.OrderNo))
            {
                throw new InvalidAdminRequestException($"Duplicate question order number {q.OrderNo}.");
            }

            if (q.Choices.Count == 0)
            {
                throw new InvalidAdminRequestException($"Question at order {q.OrderNo} must have at least one choice.");
            }
        }

        // --- Diff against the current graph ---
        var requestQuestionIds = request.Questions
            .Where(q => q.QuestionId.HasValue)
            .Select(q => new QuestionId(q.QuestionId!.Value))
            .ToHashSet();
        var questionIdsToDelete = existing.Questions
            .Select(q => q.Id)
            .Where(id => !requestQuestionIds.Contains(id))
            .ToList();

        var choiceIdsToDelete = new List<ChoiceId>();
        foreach (var q in request.Questions.Where(q => q.QuestionId.HasValue))
        {
            var existingQuestion = existing.Questions.First(eq => eq.Id == new QuestionId(q.QuestionId!.Value));
            var requestChoiceIds = q.Choices
                .Where(c => c.ChoiceId.HasValue)
                .Select(c => new ChoiceId(c.ChoiceId!.Value))
                .ToHashSet();
            choiceIdsToDelete.AddRange(existingQuestion.Choices.Select(c => c.Id).Where(id => !requestChoiceIds.Contains(id)));
        }

        var existingScoringRules = await _instrumentRepository.GetScoringRulesAsync(instrumentId, cancellationToken);
        var requestScoringRuleIds = request.ScoringRules
            .Where(r => r.ScoringRuleId.HasValue)
            .Select(r => r.ScoringRuleId!.Value)
            .ToHashSet();
        var scoringRuleIdsToDelete = existingScoringRules
            .Select(r => r.Id)
            .Where(id => !requestScoringRuleIds.Contains(id))
            .ToList();

        // --- Safety checks: nothing below this point mutates anything, so a
        // failure here leaves the instrument completely untouched. ---
        if (questionIdsToDelete.Count > 0)
        {
            var answered = await _responseRepository.GetAnsweredQuestionIdsAsync(questionIdsToDelete, cancellationToken);
            if (answered.Count > 0)
            {
                var texts = existing.Questions.Where(q => answered.Contains(q.Id)).Select(q => q.Text);
                throw new InvalidAdminRequestException(
                    $"Cannot delete question(s) that already have recorded answers: {string.Join("; ", texts)}");
            }

            var flowTransitions = await _flowTransitionRepository.GetAllAsync(cancellationToken);
            var stillReferenced = flowTransitions.Any(t => t.QuestionId.HasValue && questionIdsToDelete.Contains(t.QuestionId.Value));
            if (stillReferenced)
            {
                throw new InvalidAdminRequestException(
                    "Cannot delete a question that a flow transition still references — remove that transition first.");
            }
        }

        if (choiceIdsToDelete.Count > 0)
        {
            var answered = await _responseRepository.GetAnsweredChoiceIdsAsync(choiceIdsToDelete, cancellationToken);
            if (answered.Count > 0)
            {
                throw new InvalidAdminRequestException("Cannot delete a choice that already has recorded answers.");
            }
        }

        var oldLevels = existingScoringRules.Select(r => r.Level).ToHashSet();
        var newLevels = request.ScoringRules.Select(r => r.Level).ToHashSet();
        var levelsGoingAway = oldLevels.Except(newLevels).ToList();
        if (levelsGoingAway.Count > 0)
        {
            var levelsInUse = await _resultRepository.GetLevelsInUseAsync(instrumentId, cancellationToken);
            var blockedLevels = levelsGoingAway.Where(levelsInUse.Contains).ToList();
            if (blockedLevels.Count > 0)
            {
                throw new InvalidAdminRequestException(
                    $"Cannot remove or rename scoring level(s) that existing results already reference: {string.Join(", ", blockedLevels)}");
            }
        }

        // --- Everything checked out — apply it ---
        var questionSpecs = request.Questions
            .Select(q => new QuestionUpdateSpec(
                q.QuestionId,
                q.Text,
                q.OrderNo,
                q.Choices.Select(c => new ChoiceUpdateSpec(c.ChoiceId, c.Label, c.Score, c.OrderNo)).ToList()))
            .ToList();

        var scoringRuleSpecs = request.ScoringRules
            .Select(r => new ScoringRuleUpdateSpec(r.ScoringRuleId, r.Min, r.Max, r.Level, r.Interpretation, r.Advice))
            .ToList();

        await _instrumentRepository.UpdateFullInstrumentAsync(
            instrumentId,
            code,
            request.Name,
            request.Version,
            request.Source,
            request.IsActive,
            questionSpecs,
            questionIdsToDelete,
            choiceIdsToDelete,
            scoringRuleSpecs,
            scoringRuleIdsToDelete,
            cancellationToken);

        return await _getInstrumentFullDetailUseCase.ExecuteAsync(instrumentId, cancellationToken);
    }
}
