using System.ComponentModel.DataAnnotations;


namespace GreenSpace.Application.DTOs.VNPay
{
    public class VNPayRequestDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [Range(1000, double.MaxValue, ErrorMessage = "Amount must be at least 1,000 VND")]
        public decimal Amount { get; set; }

        [MaxLength(255)]
        public string? OrderDescription { get; set; }

        [MaxLength(50)]
        public string? BankCode { get; set; }

        //public string? IpAddress { get; set; }
    }
}
