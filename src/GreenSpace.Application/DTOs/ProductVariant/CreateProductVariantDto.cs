using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.ProductVariant
{
    public class CreateProductVariantDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public string Sku { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public string ImageUrl { get; set; }
        public string Color { get; set; }
        public string SizeOrModel { get; set; }
    }
}
