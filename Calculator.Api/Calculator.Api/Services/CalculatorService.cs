namespace Calculator.Api.Services;

public sealed class CalculatorService : ICalculatorService
{
    public double Add(IReadOnlyList<double> numbers)
    {
        ValidateInput(numbers);
        return numbers.Sum();
    }

    public double Subtract(IReadOnlyList<double> numbers)
    {
        ValidateInput(numbers);
        return numbers.Skip(1).Aggregate(numbers[0], (result, number) => result - number);
    }

    public double Multiply(IReadOnlyList<double> numbers)
    {
        ValidateInput(numbers);
        return numbers.Skip(1).Aggregate(numbers[0], (result, number) => result * number);
    }

    public double Divide(IReadOnlyList<double> numbers)
    {
        ValidateInput(numbers);

        if (numbers.Skip(1).Any(number => number == 0))
        {
            throw new DivideByZeroException("Division durch null ist nicht erlaubt.");
        }

        return numbers.Skip(1).Aggregate(numbers[0], (result, number) => result / number);
    }

    private static void ValidateInput(IReadOnlyList<double> numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        if (numbers.Count < 2)
        {
            throw new ArgumentException("Es müssen mindestens zwei Zahlen angegeben werden.", nameof(numbers));
        }
    }
}
