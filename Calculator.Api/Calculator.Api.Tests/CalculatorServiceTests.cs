using Calculator.Api.Services;

namespace Calculator.Api.Tests;

/// <summary>Unit-Tests für die Rechenlogik des <see cref="CalculatorService"/>.</summary>
public class CalculatorServiceTests
{
    private readonly CalculatorService _service = new();

    public static TheoryData<string> Operationen => new("add", "subtract", "multiply", "divide");

    private double Invoke(string op, double[] numbers) => op switch
    {
        "add" => _service.Add(numbers),
        "subtract" => _service.Subtract(numbers),
        "multiply" => _service.Multiply(numbers),
        "divide" => _service.Divide(numbers),
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };

    [Theory]
    [InlineData(new double[] { 1, 2 }, 3)]
    [InlineData(new double[] { 1, 2, 3, 4 }, 10)]
    [InlineData(new double[] { -5, 2.5 }, -2.5)]
    public void Add_liefert_Summe(double[] numbers, double expected)
        => Assert.Equal(expected, _service.Add(numbers));

    [Theory]
    [InlineData(new double[] { 10, 4 }, 6)]
    [InlineData(new double[] { 10, 4, 3 }, 3)]
    [InlineData(new double[] { -1, -1 }, 0)]
    public void Subtract_rechnet_linksassoziativ(double[] numbers, double expected)
        => Assert.Equal(expected, _service.Subtract(numbers));

    [Theory]
    [InlineData(new double[] { 3, 4 }, 12)]
    [InlineData(new double[] { 2, 3, 4 }, 24)]
    [InlineData(new double[] { 5, 0 }, 0)]
    [InlineData(new double[] { -2, 2.5 }, -5)]
    public void Multiply_liefert_Produkt(double[] numbers, double expected)
        => Assert.Equal(expected, _service.Multiply(numbers));

    [Theory]
    [InlineData(new double[] { 10, 4 }, 2.5)]
    [InlineData(new double[] { 100, 5, 2 }, 10)]
    [InlineData(new double[] { -9, 3 }, -3)]
    [InlineData(new double[] { 0, 5 }, 0)]
    public void Divide_rechnet_verkettet(double[] numbers, double expected)
        => Assert.Equal(expected, _service.Divide(numbers));

    [Fact]
    public void Add_beruecksichtigt_Gleitkomma_Toleranz()
        => Assert.Equal(0.3, _service.Add([0.1, 0.2]), 1e-10);

    [Theory]
    [InlineData(new double[] { 10, 0 })]
    [InlineData(new double[] { 10, 2, 0 })]
    public void Divide_wirft_bei_Division_durch_null(double[] numbers)
        => Assert.Throws<DivideByZeroException>(() => _service.Divide(numbers));

    [Fact]
    public void Add_wirft_bei_Ueberlauf()
        => Assert.Throws<OverflowException>(() => _service.Add([1e308, 1e308]));

    [Fact]
    public void Subtract_wirft_bei_negativem_Ueberlauf()
        => Assert.Throws<OverflowException>(() => _service.Subtract([-1e308, 1e308]));

    [Fact]
    public void Multiply_wirft_bei_Ueberlauf()
        => Assert.Throws<OverflowException>(() => _service.Multiply([1e308, 1e308]));

    [Fact]
    public void Divide_wirft_bei_Ueberlauf()
        => Assert.Throws<OverflowException>(() => _service.Divide([1e308, 1e-308]));

    [Theory]
    [MemberData(nameof(Operationen))]
    public void Operation_wirft_bei_nur_einer_Zahl(string op)
        => Assert.Throws<ArgumentException>(() => Invoke(op, [42]));

    [Theory]
    [MemberData(nameof(Operationen))]
    public void Operation_wirft_bei_leerem_Array(string op)
        => Assert.Throws<ArgumentException>(() => Invoke(op, []));

    [Theory]
    [MemberData(nameof(Operationen))]
    public void Operation_wirft_bei_null(string op)
        => Assert.Throws<ArgumentNullException>(() => Invoke(op, null!));
}
