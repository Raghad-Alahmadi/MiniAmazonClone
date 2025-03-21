using System.ComponentModel.DataAnnotations;

public class OrderItem
{
    public int OrderItemId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
}
