using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenSpace.Application.DTOs.PayOS
{
    public class PayOSRequestDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
