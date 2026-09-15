namespace Calculator.Api.Services;

public sealed class CalculatorService : ICalculatorService
{
    public decimal Add(IReadOnlyList<decimal> numbers)
    {
        ValidateInput(numbers);
        return EnsureFinite(numbers.Sum());
    }

    public decimal Subtract(IReadOnlyList<decimal> numbers)
    {
        ValidateInput(numbers);
        return EnsureFinite(numbers.Skip(1).Aggregate(numbers[0], (result, number) => result - number));
    }

    public decimal Multiply(IReadOnlyList<decimal> numbers)
    {
        ValidateInput(numbers);
        return EnsureFinite(numbers.Skip(1).Aggregate(numbers[0], (result, number) => result * number));
    }

    public decimal Divide(IReadOnlyList<decimal> numbers)
    {
        ValidateInput(numbers);

        if (numbers.Skip(1).Any(number => number == 0))
        {
            throw new DivideByZeroException("Division durch null ist nicht erlaubt.");
        }

        return EnsureFinite(numbers.Skip(1).Aggregate(numbers[0], (result, number) => result / number));
    }

    private static decimal EnsureFinite(decimal result)
    {
        if (result == decimal.MaxValue || result == decimal.MinValue)
        {
            throw new OverflowException("Das Ergebnis liegt außerhalb des darstellbaren Zahlenbereichs.");
        }

        return result;
    }

    private static void ValidateInput(IReadOnlyList<decimal> numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        if (numbers.Count < 2)
        {
            throw new ArgumentException("Es müssen mindestens zwei Zahlen angegeben werden.", nameof(numbers));
        }
    }
}
