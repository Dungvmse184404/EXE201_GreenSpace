using System;
using System.Collections.Generic;
using GreenSpace.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Attribute = GreenSpace.Domain.Models.Attribute;

namespace GreenSpace.Infrastructure.Persistence.Contexts;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attribute> Attributes { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attribute>(entity =>
        {
            entity.HasKey(e => e.AttributeId).HasName("attributes_pkey");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("carts_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_carts_user");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("cart_items_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems).HasConstraintName("fk_cart_items_cart");

            entity.HasOne(d => d.Variant).WithMany(p => p.CartItems).HasConstraintName("fk_cart_items_variant");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("categories_pkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_category_parent");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("orders_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasDefaultValueSql("'Pending'::character varying");

            entity.HasOne(d => d.User).WithMany(p => p.Orders).HasConstraintName("fk_orders_user");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("order_items_pkey");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasConstraintName("fk_order_items_order");

            entity.HasOne(d => d.Variant).WithMany(p => p.OrderItems)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_order_items_variant");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments).HasConstraintName("fk_payments_order");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("products_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_products_category");
        });

        modelBuilder.Entity<ProductAttributeValue>(entity =>
        {
            entity.HasKey(e => e.ValueId).HasName("product_attribute_values_pkey");

            entity.HasOne(d => d.Attribute).WithMany(p => p.ProductAttributeValues).HasConstraintName("fk_pav_attribute");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttributeValues).HasConstraintName("fk_pav_product");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("product_variants_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants).HasConstraintName("fk_variants_product");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("promotions_pkey");

            entity.Property(e => e.CreateAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Order).WithMany(p => p.Promotions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_promotions_order");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("ratings_pkey");

            entity.Property(e => e.CreateDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Product).WithMany(p => p.Ratings).HasConstraintName("fk_ratings_product");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings).HasConstraintName("fk_ratings_user");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.Property(e => e.CreateAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Role).HasDefaultValueSql("'Customer'::character varying");
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("user_address_pkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses).HasConstraintName("fk_user_address_users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
