using System.ComponentModel.DataAnnotations;

public class CurrenciesRequest
{
    [Required]
    public string Provider { get; set; } = "Frankfurter";
}
