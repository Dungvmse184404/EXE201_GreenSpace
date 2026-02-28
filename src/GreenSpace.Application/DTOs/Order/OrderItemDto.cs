using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Order
{
    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public Guid? VariantId { get; set; }
        public string? VariantSku { get; set; }
        public string? Color { get; set; }
        public string? SizeOrModel { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal SubTotal => Quantity * PriceAtPurchase;
    }

    public class CreateOrderItemDto
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }


}
