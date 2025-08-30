using System.ComponentModel.DataAnnotations;

namespace MyStoreManagement.Application.Dtos.Orders;

public class OrderCreateRequest
{
    [Required(ErrorMessage = "Phonenumber is required")]
    public string PhoneNumber { get; set; }
    
    public List<OrderDetails> OrderDetails { get; set; }
}

public class OrderDetails
{
    [Required(ErrorMessage = "ProductId is required")]
    public int OrderTypeId { get; set; }
    
    [Required(ErrorMessage = "Quantity is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}