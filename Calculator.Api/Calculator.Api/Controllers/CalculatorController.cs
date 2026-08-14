using Calculator.Api.Models;
using Calculator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Calculator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CalculatorController : ControllerBase
{
    private readonly ICalculatorService _calculatorService;

    public CalculatorController(ICalculatorService calculatorService)
    {
        _calculatorService = calculatorService;
    }

    /// <summary>Addiert zwei oder mehr Zahlen.</summary>
    /// <param name="request">Die zu addierenden Zahlen.</param>
    /// <returns>Das Ergebnis der Addition.</returns>
    /// <response code="200">Berechnung erfolgreich.</response>
    /// <response code="400">Ungültige Eingabe.</response>
    [HttpPost("add")]
    [ProducesResponseType<CalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<CalculationResponse> Add([FromBody] CalculationRequest request)
        => Calculate(OperationNames.Addition, request, _calculatorService.Add);

    /// <summary>Subtrahiert alle weiteren Zahlen von der ersten Zahl.</summary>
    /// <param name="request">Die Zahlen für die Subtraktion.</param>
    /// <returns>Das Ergebnis der Subtraktion.</returns>
    /// <response code="200">Berechnung erfolgreich.</response>
    /// <response code="400">Ungültige Eingabe.</response>
    [HttpPost("subtract")]
    [ProducesResponseType<CalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<CalculationResponse> Subtract([FromBody] CalculationRequest request)
        => Calculate(OperationNames.Subtraktion, request, _calculatorService.Subtract);

    /// <summary>Multipliziert zwei oder mehr Zahlen.</summary>
    /// <param name="request">Die zu multiplizierenden Zahlen.</param>
    /// <returns>Das Ergebnis der Multiplikation.</returns>
    /// <response code="200">Berechnung erfolgreich.</response>
    /// <response code="400">Ungültige Eingabe.</response>
    [HttpPost("multiply")]
    [ProducesResponseType<CalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<CalculationResponse> Multiply([FromBody] CalculationRequest request)
        => Calculate(OperationNames.Multiplikation, request, _calculatorService.Multiply);

    /// <summary>Dividiert die erste Zahl nacheinander durch alle weiteren Zahlen.</summary>
    /// <param name="request">Die Zahlen für die Division.</param>
    /// <returns>Das Ergebnis der Division.</returns>
    /// <response code="200">Berechnung erfolgreich.</response>
    /// <response code="400">Ungültige Eingabe (z. B. Division durch null).</response>
    [HttpPost("divide")]
    [ProducesResponseType<CalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<CalculationResponse> Divide([FromBody] CalculationRequest request)
        => Calculate(OperationNames.Division, request, _calculatorService.Divide);

    // Fachliche Ausnahmen (DivideByZero, Overflow, Argument) behandelt zentral der CalculationExceptionHandler.
    private ActionResult<CalculationResponse> Calculate(
        string operation,
        CalculationRequest request,
        Func<IReadOnlyList<double>, double> calculation)
        => Ok(new CalculationResponse(operation, request.Numbers, calculation(request.Numbers)));
}
