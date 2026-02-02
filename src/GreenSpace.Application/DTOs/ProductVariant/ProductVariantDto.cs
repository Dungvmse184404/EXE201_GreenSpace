using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.ProductVariant
{
    public class ProductVariantDto
    {
        public Guid VariantId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? Color { get; set; }
        public string? SizeOrModel { get; set; }
        public bool? IsActive { get; set; }
    }
}
