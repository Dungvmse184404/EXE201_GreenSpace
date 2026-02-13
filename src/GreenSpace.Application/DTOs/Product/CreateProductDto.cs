using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Product
{
    public class CreateProductDto
    {
        [Required, MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        public string ThumbnailUrl { get; set; }

        public int CategoryId { get; set; }
        //public int? BrandId { get; set; }
    }
}

