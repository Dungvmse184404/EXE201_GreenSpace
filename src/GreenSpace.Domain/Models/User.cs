using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("users")]
[Index("Email", Name = "users_email_key", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column("full_name")]
    [StringLength(255)]
    public string? FullName { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("role")]
    [StringLength(20)]
    public string? Role { get; set; }

    [Column("date_of_birth", TypeName = "timestamp without time zone")]
    public DateTime? DateOfBirth { get; set; }

    [Column("gender")]
    [StringLength(10)]
    public string? Gender { get; set; }

    [Column("is_active")]
    public bool? IsActive { get; set; }

    [Column("create_at", TypeName = "timestamp without time zone")]
    public DateTime? CreateAt { get; set; }

    [Column("update_at", TypeName = "timestamp without time zone")]
    public DateTime? UpdateAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [InverseProperty("User")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [InverseProperty("User")]
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    [InverseProperty("User")]
    public virtual ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
}
