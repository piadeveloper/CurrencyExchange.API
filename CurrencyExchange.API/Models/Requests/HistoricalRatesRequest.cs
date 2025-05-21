using System.ComponentModel.DataAnnotations;

public class HistoricalRatesRequest
{
    [Required]
    public string Provider { get; set; } = "Frankfurter";

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; set; } = "EUR";

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; } = 1.0m;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 20;
}
