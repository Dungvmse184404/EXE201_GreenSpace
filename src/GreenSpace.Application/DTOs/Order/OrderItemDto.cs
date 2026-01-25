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
        public string VariantSku { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal SubTotal => Quantity * PriceAtPurchase; //field này không map từ db mà tính toán từ các field khác
    }

    public class CreateOrderItemDto
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }


}
