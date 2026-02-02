using System.ComponentModel.DataAnnotations;

namespace GreenSpace.Application.DTOs.Cart
{
    public class CartDto
    {
        public Guid CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class CartItemDto
    {
        public Guid CartItemId { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal => Quantity * Price;
    }

    public class ModifyCartItemDto
    {
        [Required]
        public Guid VariantId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }


}