using System.ComponentModel.DataAnnotations;

public class Order
{
    public int OrderId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    [Required]
    public string Status { get; set; }

    [Required]
    public List<OrderItem> OrderItems { get; set; }
}
