namespace Calculator.Api.Services;

/// <summary>
/// Stellt die Standard-Rechenarten bereit.
/// </summary>
public interface ICalculatorService
{
    decimal Add(IReadOnlyList<decimal> numbers);

    decimal Subtract(IReadOnlyList<decimal> numbers);

    decimal Multiply(IReadOnlyList<decimal> numbers);

    decimal Divide(IReadOnlyList<decimal> numbers);
}
