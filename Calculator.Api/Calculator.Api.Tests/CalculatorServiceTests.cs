using Calculator.Api.Services;

namespace Calculator.Api.Tests;

/// <summary>Unit-Tests für die Rechenlogik des <see cref="CalculatorService"/> (decimal-basiert).</summary>
public class CalculatorServiceTests
{
    private readonly CalculatorService _service = new();

    public static TheoryData<string> Operationen => new("add", "subtract", "multiply", "divide");

    private decimal Invoke(string op, decimal[] numbers) => op switch
    {
        "add" => _service.Add(numbers),
        "subtract" => _service.Subtract(numbers),
        "multiply" => _service.Multiply(numbers),
        "divide" => _service.Divide(numbers),
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };

    public static TheoryData<decimal[], decimal> AddCases => new()
    {
        { [1m, 2m], 3m },
        { [1m, 2m, 3m, 4m], 10m },
        { [-5m, 2.5m], -2.5m },
        { [0.1m, 0.2m], 0.3m },
        { [0.1m, 0.7m], 0.8m },
        { [1234567890.1234567890m, 0.0000000001m], 1234567890.1234567891m },
    };

    public static TheoryData<decimal[], decimal> SubtractCases => new()
    {
        { [10m, 4m], 6m },
        { [10m, 4m, 3m], 3m },
        { [-1m, -1m], 0m },
        { [1m, 0.9m], 0.1m },
        { [0.3m, 0.1m], 0.2m },
    };

    public static TheoryData<decimal[], decimal> MultiplyCases => new()
    {
        { [3m, 4m], 12m },
        { [2m, 3m, 4m], 24m },
        { [5m, 0m], 0m },
        { [-2m, 2.5m], -5m },
        { [0.1m, 0.1m], 0.01m },
        { [1.1m, 1.1m], 1.21m },
    };

    public static TheoryData<decimal[], decimal> DivideCases => new()
    {
        { [10m, 4m], 2.5m },
        { [100m, 5m, 2m], 10m },
        { [-9m, 3m], -3m },
        { [0m, 5m], 0m },
        { [1m, 8m], 0.125m },
        { [1m, 3m], 0.3333333333333333333333333333m },
    };

    public static TheoryData<decimal[]> DivideByZeroCases => new()
    {
        new decimal[] { 10m, 0m },
        new decimal[] { 10m, 2m, 0m },
        new decimal[] { 0m, 0m },
    };

    [Theory]
    [MemberData(nameof(AddCases))]
    public void Add_liefert_Summe(decimal[] numbers, decimal expected)
        => Assert.Equal(expected, _service.Add(numbers));

    [Theory]
    [MemberData(nameof(SubtractCases))]
    public void Subtract_rechnet_linksassoziativ(decimal[] numbers, decimal expected)
        => Assert.Equal(expected, _service.Subtract(numbers));

    [Theory]
    [MemberData(nameof(MultiplyCases))]
    public void Multiply_liefert_Produkt(decimal[] numbers, decimal expected)
        => Assert.Equal(expected, _service.Multiply(numbers));

    [Theory]
    [MemberData(nameof(DivideCases))]
    public void Divide_rechnet_verkettet(decimal[] numbers, decimal expected)
        => Assert.Equal(expected, _service.Divide(numbers));

    [Fact]
    public void Add_rechnet_exakt_ohne_Gleitkommafehler()
        // Bei double wäre 0.1 + 0.2 != 0.3 – decimal muss exakt sein.
        => Assert.Equal(0.3m, _service.Add([0.1m, 0.2m]));

    [Fact]
    public void Subtract_rechnet_exakt_ohne_Gleitkommafehler()
        => Assert.Equal(0.1m, _service.Subtract([1m, 0.9m]));

    [Fact]
    public void Multiply_behaelt_Nachkommastellen_bei()
        => Assert.Equal(0.001m, _service.Multiply([0.1m, 0.01m]));

    [Fact]
    public void Divide_liefert_maximale_decimal_Praezision()
    {
        var result = _service.Divide([2m, 3m]);

        Assert.Equal(0.6666666666666666666666666667m, result);
        Assert.Equal(28, result.Scale);
    }

    [Fact]
    public void Add_nutzt_Vorzeichen_und_Null_korrekt()
        => Assert.Equal(0m, _service.Add([decimal.MaxValue, decimal.MinValue]));

    [Theory]
    [MemberData(nameof(DivideByZeroCases))]
    public void Divide_wirft_bei_Division_durch_null(decimal[] numbers)
        => Assert.Throws<DivideByZeroException>(() => _service.Divide(numbers));

    [Fact]
    public void Divide_erlaubt_Null_als_Dividend()
        => Assert.Equal(0m, _service.Divide([0m, 7m]));

    [Fact]
    public void Add_wirft_bei_Ueberlauf()
        => Assert.Throws<OverflowException>(() => _service.Add([decimal.MaxValue, decimal.MaxValue]));

    [Fact]
    public void Add_wirft_bei_Ueberlauf_durch_kleinen_Wert()
        => Assert.Throws<OverflowException>(() => _service.Add([decimal.MaxValue, 1m]));

    [Fact]
    public void Subtract_wirft_bei_negativem_Ueberlauf()
        => Assert.Throws<OverflowException>(() => _service.Subtract([decimal.MinValue, decimal.MaxValue]));

    [Fact]
    public void Multiply_wirft_bei_Ueberlauf()
        => Assert.Throws<OverflowException>(() => _service.Multiply([decimal.MaxValue, 2m]));

    [Fact]
    public void Multiply_wirft_bei_Ueberlauf_grosser_Zwischenergebnisse()
        => Assert.Throws<OverflowException>(() => _service.Multiply([1_000_000_000_000_000m, 1_000_000_000_000_000m]));

    [Fact]
    public void Divide_wirft_bei_Ueberlauf()
        => Assert.Throws<OverflowException>(() => _service.Divide([decimal.MaxValue, 0.5m]));

    [Fact]
    public void Divide_wirft_bei_Ueberlauf_durch_kleinsten_Divisor()
        // 1e-28 ist der kleinste darstellbare positive decimal-Wert
        => Assert.Throws<OverflowException>(() => _service.Divide([decimal.MaxValue, 0.0000000000000000000000000001m]));

    [Theory]
    [MemberData(nameof(Operationen))]
    public void Operation_wirft_bei_nur_einer_Zahl(string op)
        => Assert.Throws<ArgumentException>(() => Invoke(op, [42m]));

    [Theory]
    [MemberData(nameof(Operationen))]
    public void Operation_wirft_bei_leerem_Array(string op)
        => Assert.Throws<ArgumentException>(() => Invoke(op, []));

    [Theory]
    [MemberData(nameof(Operationen))]
    public void Operation_wirft_bei_null(string op)
        => Assert.Throws<ArgumentNullException>(() => Invoke(op, null!));

    [Fact]
    public void Subtract_verarbeitet_Grenzwerte_ohne_Ueberlauf()
        => Assert.Equal(decimal.MaxValue - 1m, _service.Subtract([decimal.MaxValue, 1m]));
}
