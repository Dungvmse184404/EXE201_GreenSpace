using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenSpace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPricingAndVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================
            // ORDERS TABLE - Add columns if not exist
            // ============================================
            migrationBuilder.Sql(@"
                ALTER TABLE orders ADD COLUMN IF NOT EXISTS sub_total NUMERIC(10, 2) DEFAULT 0;
                ALTER TABLE orders ADD COLUMN IF NOT EXISTS discount NUMERIC(10, 2) DEFAULT 0;
                ALTER TABLE orders ADD COLUMN IF NOT EXISTS voucher_code VARCHAR(50);
                ALTER TABLE orders ADD COLUMN IF NOT EXISTS shipping_fee NUMERIC(10, 2) DEFAULT 0;
                ALTER TABLE orders ADD COLUMN IF NOT EXISTS final_amount NUMERIC(10, 2) DEFAULT 0;
                ALTER TABLE orders ADD COLUMN IF NOT EXISTS note VARCHAR(500);
                ALTER TABLE orders ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP;
            ");

            // ============================================
            // PROMOTIONS TABLE - Add columns if not exist
            // ============================================
            migrationBuilder.Sql(@"
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS code VARCHAR(50);
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS name VARCHAR(100);
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS description VARCHAR(500);
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS discount_value NUMERIC(10, 2) DEFAULT 0;
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS max_discount NUMERIC(10, 2);
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS min_order_value NUMERIC(10, 2);
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS max_usage INTEGER;
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS used_count INTEGER DEFAULT 0;
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS created_at TIMESTAMP;
                ALTER TABLE promotions ADD COLUMN IF NOT EXISTS updated_at TIMESTAMP;
            ");

            // Update is_active to non-nullable
            migrationBuilder.Sql(@"
                UPDATE promotions SET is_active = true WHERE is_active IS NULL;
                ALTER TABLE promotions ALTER COLUMN is_active SET DEFAULT true;
                ALTER TABLE promotions ALTER COLUMN is_active SET NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "description",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "discount_value",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "max_discount",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "max_usage",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "min_order_value",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "name",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "used_count",
                table: "promotions");

            migrationBuilder.DropColumn(
                name: "discount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "final_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "note",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_fee",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "sub_total",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "voucher_code",
                table: "orders");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "promotions",
                type: "boolean",
                nullable: true,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
