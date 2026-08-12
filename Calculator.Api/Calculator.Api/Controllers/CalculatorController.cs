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

    [HttpPost("add")]
    [ProducesResponseType<CalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CalculationResponse> Add([FromBody] CalculationRequest request)
        => Calculate("Addition", request, _calculatorService.Add);

    [HttpPost("subtract")]
    [ProducesResponseType<CalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CalculationResponse> Subtract([FromBody] CalculationRequest request)
        => Calculate("Subtraktion", request, _calculatorService.Subtract);

    [HttpPost("multiply")]
    [ProducesResponseType<CalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CalculationResponse> Multiply([FromBody] CalculationRequest request)
        => Calculate("Multiplikation", request, _calculatorService.Multiply);

    [HttpPost("divide")]
    [ProducesResponseType<CalculationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CalculationResponse> Divide([FromBody] CalculationRequest request)
        => Calculate("Division", request, _calculatorService.Divide);

    private ActionResult<CalculationResponse> Calculate(
        string operation,
        CalculationRequest request,
        Func<IReadOnlyList<double>, double> calculation)
    {
        try
        {
            var result = calculation(request.Numbers);
            return Ok(new CalculationResponse(operation, request.Numbers, result));
        }
        catch (DivideByZeroException ex)
        {
            return Problem(
                title: "Ungültige Berechnung",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Ungültige Eingabe",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
