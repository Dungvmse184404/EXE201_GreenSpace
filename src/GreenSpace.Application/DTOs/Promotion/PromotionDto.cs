using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.Promotion
{
    public class PromotionDto
    {
        public Guid PromotionId { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreatePromotionDto
    {
        [Required]
        public string DiscountType { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }



}
