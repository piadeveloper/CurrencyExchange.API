using System.ComponentModel.DataAnnotations;

public class TimeSeriesRequest
{
    [Required]
    public string Provider { get; set; } = "Frankfurter";

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; } = 1.0m;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 20;
}
