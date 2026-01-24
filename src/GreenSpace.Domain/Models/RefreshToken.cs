using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("refresh_tokens")]
[Index("Token", Name = "idx_refresh_tokens_token")]
[Index("Token", Name = "refresh_tokens_token_key", IsUnique = true)]
public partial class RefreshToken
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("token")]
    [StringLength(500)]
    public string Token { get; set; } = null!;

    [Column("jwt_id")]
    [StringLength(255)]
    public string JwtId { get; set; } = null!;

    [Column("is_used")]
    public bool? IsUsed { get; set; }

    [Column("is_revoked")]
    public bool? IsRevoked { get; set; }

    [Column("added_date", TypeName = "timestamp without time zone")]
    public DateTime? AddedDate { get; set; }

    [Column("expiry_date", TypeName = "timestamp without time zone")]
    public DateTime ExpiryDate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("RefreshTokens")]
    public virtual User User { get; set; } = null!;
}
