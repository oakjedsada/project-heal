using Microsoft.AspNetCore.Diagnostics;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.Exceptions;

namespace MindCheck.Api.ErrorHandling;

public sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var statusCode = exception switch
        {
            SessionNotFoundException => StatusCodes.Status404NotFound,
            InstrumentNotFoundException => StatusCodes.Status404NotFound,
            SessionAlreadyFinishedException => StatusCodes.Status409Conflict,
            QuestionNotInCurrentInstrumentException => StatusCodes.Status400BadRequest,
            ChoiceNotFoundException => StatusCodes.Status400BadRequest,
            IncompleteAssessmentException => StatusCodes.Status400BadRequest,
            ScoringRuleNotFoundException => StatusCodes.Status500InternalServerError,
            NoStartTransitionConfiguredException => StatusCodes.Status500InternalServerError,
            DuplicateInstrumentCodeException => StatusCodes.Status409Conflict,
            InvalidAdminRequestException => StatusCodes.Status400BadRequest,
            FlowTransitionNotFoundException => StatusCodes.Status404NotFound,
            InvalidCredentialsException => StatusCodes.Status401Unauthorized,
            DuplicateUsernameException => StatusCodes.Status409Conflict,
            DuplicateEmailException => StatusCodes.Status409Conflict,
            UserNotFoundException => StatusCodes.Status404NotFound,
            SessionAccessDeniedException => StatusCodes.Status403Forbidden,
            InvalidAuthRequestException => StatusCodes.Status400BadRequest,
            _ => 0
        };

        if (statusCode == 0)
        {
            return false;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            new { title = exception.GetType().Name, detail = exception.Message },
            cancellationToken);
        return true;
    }
}
