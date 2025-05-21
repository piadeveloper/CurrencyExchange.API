using System.ComponentModel.DataAnnotations;

public class LatestRatesRequest
{
    [Required]
    public string Provider { get; set; } = "Frankfurter";

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount should be greater than zero")]
    public decimal Amount { get; set; } = 1.0m;
}
