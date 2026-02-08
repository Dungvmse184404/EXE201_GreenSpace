using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("ratings")]
[Index("ProductId", Name = "IX_ratings_product_id")]
[Index("UserId", Name = "IX_ratings_user_id")]
public partial class Rating
{
    [Key]
    [Column("rating_id")]
    public Guid RatingId { get; set; }

    [Column("product_id")]
    public Guid ProductId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("stars")]
    [Precision(2, 1)]
    public decimal? Stars { get; set; }

    [Column("create_date", TypeName = "timestamp without time zone")]
    public DateTime? CreateDate { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Ratings")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Ratings")]
    public virtual User User { get; set; } = null!;
}
