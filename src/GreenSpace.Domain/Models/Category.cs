using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GreenSpace.Domain.Models;

[Table("categories")]
public partial class Category
{
    [Key]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("parent_id")]
    public int? ParentId { get; set; }

    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Column("slug")]
    [StringLength(255)]
    public string? Slug { get; set; }

    [InverseProperty("Parent")]
    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual Category? Parent { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
