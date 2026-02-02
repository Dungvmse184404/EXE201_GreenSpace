using System.ComponentModel.DataAnnotations;

namespace GreenSpace.Application.DTOs.Rating
{
    public class RatingDto
    {
        public Guid RatingId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal? Stars { get; set; }
        public string? Comment { get; set; }
        public DateTime? CreateDate { get; set; }
    }

    public class CreateRatingDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal Stars { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }


    public class UpdateRatingDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal Stars { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }
}