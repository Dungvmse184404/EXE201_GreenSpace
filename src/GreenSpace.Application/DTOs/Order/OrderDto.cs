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

        // Shipping info
        public Guid? ShippingAddressId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }

        public string? Note { get; set; }
        public string? PaymentMethod { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
    }


    public class CreateOrderDto
    {
        /// <summary>
        /// ID dia chi da luu (optional - neu co thi lay tu UserAddress)
        /// </summary>
        public Guid? AddressId { get; set; }

        /// <summary>
        /// Dia chi giao hang (bat buoc neu khong co AddressId)
        /// </summary>
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Ten nguoi nhan hang (optional - neu khong co se lay tu user profile)
        /// </summary>
        [MaxLength(100)]
        public string? RecipientName { get; set; }

        /// <summary>
        /// So dien thoai nguoi nhan hang (optional - neu khong co se lay tu user profile)
        /// </summary>
        [MaxLength(20)]
        public string? RecipientPhone { get; set; }

        /// <summary>
        /// Ma voucher giam gia (optional)
        /// </summary>
        [MaxLength(50)]
        public string? VoucherCode { get; set; }

        /// <summary>
        /// Ghi chu don hang (optional)
        /// </summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    
}
