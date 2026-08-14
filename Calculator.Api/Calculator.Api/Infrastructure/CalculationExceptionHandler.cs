using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Infrastructure;

/// <summary>
/// Übersetzt fachliche Ausnahmen der Berechnung zentral in 400-ProblemDetails-Antworten (RFC 9457).
/// Unbekannte Ausnahmen werden nicht behandelt und laufen in die Standard-500-ProblemDetails.
/// </summary>
internal sealed class CalculationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var title = exception switch
        {
            DivideByZeroException or OverflowException => "Ungültige Berechnung",
            ArgumentException => "Ungültige Eingabe",
            _ => null,
        };

        if (title is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Title = title,
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest,
            },
        });
    }
}
