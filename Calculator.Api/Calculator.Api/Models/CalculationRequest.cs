using System.ComponentModel.DataAnnotations;

namespace Calculator.Api.Models;

/// <summary>
/// Request-Body für eine Berechnung mit zwei oder mehr Zahlen.
/// </summary>
public sealed record CalculationRequest
{
    [Required]
    [MinLength(2, ErrorMessage = "Es müssen mindestens zwei Zahlen angegeben werden.")]
    [MaxLength(1000, ErrorMessage = "Es dürfen höchstens 1000 Zahlen angegeben werden.")]
    public required IReadOnlyList<double> Numbers { get; init; }
}
