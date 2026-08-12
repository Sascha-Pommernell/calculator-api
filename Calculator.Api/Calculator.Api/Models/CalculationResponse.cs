namespace Calculator.Api.Models;

/// <summary>
/// Ergebnis einer Berechnung.
/// </summary>
public sealed record CalculationResponse(string Operation, IReadOnlyList<double> Numbers, double Result);
