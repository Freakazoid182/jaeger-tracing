namespace telemetry.contracts;

using System.ComponentModel.DataAnnotations;

public class CreateProduct
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string? Name { get; set; }
}
