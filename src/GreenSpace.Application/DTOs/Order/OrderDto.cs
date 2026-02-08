using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Order
{
    public class OrderDto
    {
        public Guid OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        // Price breakdown (simplified)
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public string? VoucherCode { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal FinalAmount { get; set; }

        // Backward compatible
        public decimal TotalAmount { get; set; }

        public string ShippingAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }


    public class CreateOrderDto
    {
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Mã voucher giảm giá (optional)
        /// </summary>
        [MaxLength(50)]
        public string? VoucherCode { get; set; }

        /// <summary>
        /// Ghi chú đơn hàng (optional)
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    
}
