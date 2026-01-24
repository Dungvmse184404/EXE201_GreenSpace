using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("user_address")]
public partial class UserAddress
{
    [Key]
    [Column("address_id")]
    public Guid AddressId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("address")]
    [StringLength(255)]
    public string Address { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserAddresses")]
    public virtual User User { get; set; } = null!;
}
