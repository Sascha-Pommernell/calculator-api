namespace Calculator.Api.Services;

/// <summary>
/// Stellt die Standard-Rechenarten bereit.
/// </summary>
public interface ICalculatorService
{
    double Add(IReadOnlyList<double> numbers);

    double Subtract(IReadOnlyList<double> numbers);

    double Multiply(IReadOnlyList<double> numbers);

    double Divide(IReadOnlyList<double> numbers);
}
