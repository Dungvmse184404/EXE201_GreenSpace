using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Order
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
