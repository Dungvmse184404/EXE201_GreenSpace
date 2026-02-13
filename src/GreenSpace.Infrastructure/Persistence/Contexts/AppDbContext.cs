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

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    public virtual DbSet<DiagnosisCache> DiagnosisCaches { get; set; }

    public virtual DbSet<SymptomDictionary> SymptomDictionaries { get; set; }

    // Disease Knowledge Base
    public virtual DbSet<PlantType> PlantTypes { get; set; }
    public virtual DbSet<Disease> Diseases { get; set; }
    public virtual DbSet<DiseaseSymptom> DiseaseSymptoms { get; set; }
    public virtual DbSet<PlantTypeDisease> PlantTypeDiseases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.HasPostgresExtension("pg_trgm"); // For trigram similarity search

        modelBuilder.Entity<Attribute>(entity =>
        {
            entity.HasKey(e => e.AttributeId).HasName("attributes_pkey");

            entity.Property(e => e.AttributeId).HasDefaultValueSql("gen_random_uuid()");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("carts_pkey");

            entity.Property(e => e.CartId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_carts_user");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("cart_items_pkey");

            entity.Property(e => e.CartItemId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems).HasConstraintName("fk_cart_items_cart");

            entity.HasOne(d => d.Variant).WithMany(p => p.CartItems).HasConstraintName("fk_cart_items_variant");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("categories_pkey");

            entity.Property(e => e.CategoryId).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_category_parent");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("orders_pkey");

            entity.Property(e => e.OrderId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasDefaultValueSql("'Pending'::character varying");

            entity.HasOne(d => d.User).WithMany(p => p.Orders).HasConstraintName("fk_orders_user");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("order_items_pkey");

            entity.Property(e => e.ItemId).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasConstraintName("fk_order_items_order");

            entity.HasOne(d => d.Variant).WithMany(p => p.OrderItems)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_order_items_variant");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payments_pkey");

            entity.Property(e => e.PaymentId).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments).HasConstraintName("fk_payments_order");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("products_pkey");

            entity.Property(e => e.ProductId).HasDefaultValueSql("gen_random_uuid()");
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

            entity.Property(e => e.ValueId).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.Attribute).WithMany(p => p.ProductAttributeValues).HasConstraintName("fk_pav_attribute");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductAttributeValues).HasConstraintName("fk_pav_product");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("product_variants_pkey");

            entity.Property(e => e.VariantId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants).HasConstraintName("fk_variants_product");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("promotions_pkey");

            entity.Property(e => e.PromotionId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Create unique index on Code for voucher lookup
            entity.HasIndex(e => e.Code)
                .IsUnique()
                .HasFilter("code IS NOT NULL");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => e.RatingId).HasName("ratings_pkey");

            entity.Property(e => e.RatingId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreateDate).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Product).WithMany(p => p.Ratings).HasConstraintName("fk_ratings_product");

            entity.HasOne(d => d.User).WithMany(p => p.Ratings).HasConstraintName("fk_ratings_user");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("refresh_tokens_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.AddedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens).HasConstraintName("fk_refresh_tokens_users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.Property(e => e.UserId).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreateAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Role).HasDefaultValueSql("'Customer'::character varying");
            entity.Property(e => e.UpdateAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("user_address_pkey");

            entity.Property(e => e.AddressId).HasDefaultValueSql("gen_random_uuid()");

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses).HasConstraintName("fk_user_address_users");
        });

        // =================================================================
        // DIAGNOSIS CACHE CONFIGURATION
        // =================================================================
        modelBuilder.Entity<DiagnosisCache>(entity =>
        {
            entity.ToTable("diagnosis_cache");

            entity.HasKey(e => e.Id).HasName("diagnosis_cache_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.PlantType)
                .HasColumnName("plant_type")
                .HasMaxLength(100);

            entity.Property(e => e.OriginalDescription)
                .HasColumnName("original_description")
                .IsRequired();

            entity.Property(e => e.NormalizedDescription)
                .HasColumnName("normalized_description")
                .IsRequired();

            entity.Property(e => e.Symptoms)
                .HasColumnName("symptoms")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            entity.Property(e => e.DiseaseName)
                .HasColumnName("disease_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.AiResponse)
                .HasColumnName("ai_response")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.ConfidenceScore)
                .HasColumnName("confidence_score");

            entity.Property(e => e.HitCount)
                .HasColumnName("hit_count")
                .HasDefaultValue(0);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP + INTERVAL '90 days'");

            entity.Property(e => e.CacheTtlDays)
                .HasColumnName("cache_ttl_days")
                .HasDefaultValue(90);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.HasImage)
                .HasColumnName("has_image")
                .HasDefaultValue(false);

            // Index for trigram similarity search (requires pg_trgm extension)
            entity.HasIndex(e => e.NormalizedDescription)
                .HasDatabaseName("idx_cache_normalized_desc");

            // Index for plant type lookup
            entity.HasIndex(e => e.PlantType)
                .HasDatabaseName("idx_cache_plant_type");

            // Index for active and not expired entries
            entity.HasIndex(e => new { e.IsActive, e.ExpiresAt })
                .HasDatabaseName("idx_cache_active_expires");
        });

        // =================================================================
        // SYMPTOM DICTIONARY CONFIGURATION
        // =================================================================
        modelBuilder.Entity<SymptomDictionary>(entity =>
        {
            entity.ToTable("symptom_dictionary");

            entity.HasKey(e => e.Id).HasName("symptom_dictionary_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.CanonicalName)
                .HasColumnName("canonical_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Synonyms)
                .HasColumnName("synonyms")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(50);

            entity.Property(e => e.EnglishName)
                .HasColumnName("english_name")
                .HasMaxLength(100);

            // Unique index on canonical name
            entity.HasIndex(e => e.CanonicalName)
                .IsUnique()
                .HasDatabaseName("idx_symptom_canonical_name");

            // Index for category lookup
            entity.HasIndex(e => e.Category)
                .HasDatabaseName("idx_symptom_category");
        });

        // =================================================================
        // PLANT TYPE CONFIGURATION
        // =================================================================
        modelBuilder.Entity<PlantType>(entity =>
        {
            entity.ToTable("plant_types");

            entity.HasKey(e => e.Id).HasName("plant_types_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.CommonName)
                .HasColumnName("common_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.ScientificName)
                .HasColumnName("scientific_name")
                .HasMaxLength(150);

            entity.Property(e => e.Family)
                .HasColumnName("family")
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.ImageUrl)
                .HasColumnName("image_url")
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.CommonName)
                .HasDatabaseName("idx_plant_type_common_name");
        });

        // =================================================================
        // DISEASE CONFIGURATION
        // =================================================================
        modelBuilder.Entity<Disease>(entity =>
        {
            entity.ToTable("diseases");

            entity.HasKey(e => e.Id).HasName("diseases_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.DiseaseName)
                .HasColumnName("disease_name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.EnglishName)
                .HasColumnName("english_name")
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.Severity)
                .HasColumnName("severity")
                .HasMaxLength(20)
                .HasDefaultValue("Medium");

            entity.Property(e => e.Causes)
                .HasColumnName("causes")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            entity.Property(e => e.ImmediateActions)
                .HasColumnName("immediate_actions")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            entity.Property(e => e.LongTermCare)
                .HasColumnName("long_term_care")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            entity.Property(e => e.PreventionTips)
                .HasColumnName("prevention_tips")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            entity.Property(e => e.WateringAdvice)
                .HasColumnName("watering_advice")
                .HasMaxLength(500);

            entity.Property(e => e.LightingAdvice)
                .HasColumnName("lighting_advice")
                .HasMaxLength(500);

            entity.Property(e => e.FertilizingAdvice)
                .HasColumnName("fertilizing_advice")
                .HasMaxLength(500);

            entity.Property(e => e.ImageUrls)
                .HasColumnName("image_urls")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            entity.Property(e => e.ProductKeywords)
                .HasColumnName("product_keywords")
                .HasColumnType("text[]")
                .HasDefaultValueSql("'{}'::text[]");

            entity.Property(e => e.Notes)
                .HasColumnName("notes");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.DiseaseName)
                .HasDatabaseName("idx_disease_name");
        });

        // =================================================================
        // DISEASE SYMPTOM CONFIGURATION
        // =================================================================
        modelBuilder.Entity<DiseaseSymptom>(entity =>
        {
            entity.ToTable("disease_symptoms");

            entity.HasKey(e => e.Id).HasName("disease_symptoms_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.DiseaseId)
                .HasColumnName("disease_id");

            entity.Property(e => e.SymptomId)
                .HasColumnName("symptom_id");

            entity.Property(e => e.IsPrimary)
                .HasColumnName("is_primary")
                .HasDefaultValue(false);

            entity.Property(e => e.Weight)
                .HasColumnName("weight")
                .HasDefaultValue(1.0m);

            entity.Property(e => e.AffectedPart)
                .HasColumnName("affected_part")
                .HasMaxLength(20);

            entity.HasOne(d => d.Disease)
                .WithMany(d => d.DiseaseSymptoms)
                .HasForeignKey(d => d.DiseaseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_disease_symptoms_disease");

            entity.HasOne(d => d.Symptom)
                .WithMany()
                .HasForeignKey(d => d.SymptomId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_disease_symptoms_symptom");

            entity.HasIndex(e => e.DiseaseId)
                .HasDatabaseName("idx_disease_symptoms_disease");

            entity.HasIndex(e => e.SymptomId)
                .HasDatabaseName("idx_disease_symptoms_symptom");
        });

        // =================================================================
        // PLANT TYPE DISEASE CONFIGURATION
        // =================================================================
        modelBuilder.Entity<PlantTypeDisease>(entity =>
        {
            entity.ToTable("plant_type_diseases");

            entity.HasKey(e => e.Id).HasName("plant_type_diseases_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.PlantTypeId)
                .HasColumnName("plant_type_id");

            entity.Property(e => e.DiseaseId)
                .HasColumnName("disease_id");

            entity.Property(e => e.Prevalence)
                .HasColumnName("prevalence")
                .HasMaxLength(20);

            entity.Property(e => e.Notes)
                .HasColumnName("notes");

            entity.HasOne(d => d.PlantType)
                .WithMany(p => p.PlantTypeDiseases)
                .HasForeignKey(d => d.PlantTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_plant_type_diseases_plant_type");

            entity.HasOne(d => d.Disease)
                .WithMany(d => d.PlantTypeDiseases)
                .HasForeignKey(d => d.DiseaseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_plant_type_diseases_disease");

            entity.HasIndex(e => e.PlantTypeId)
                .HasDatabaseName("idx_plant_type_diseases_plant_type");

            entity.HasIndex(e => e.DiseaseId)
                .HasDatabaseName("idx_plant_type_diseases_disease");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
