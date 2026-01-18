using GreenSpace.Application.DTOs.ProductVariant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Product
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public string ThumbnailUrl { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; } // Giả sử nếu có Brand
        public List<ProductVariantDto> Variants { get; set; } = new();
    }
}
