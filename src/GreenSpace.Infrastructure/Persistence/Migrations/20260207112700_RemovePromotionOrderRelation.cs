using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenSpace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePromotionOrderRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK and index if exist (safe for both fresh and existing DBs)
            migrationBuilder.Sql(@"
                ALTER TABLE promotions DROP CONSTRAINT IF EXISTS fk_promotions_order;
                DROP INDEX IF EXISTS ""IX_promotions_order_id"";
                ALTER TABLE promotions DROP COLUMN IF EXISTS order_id;
            ");

            // Create unique index on code for voucher lookup
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_promotions_code""
                ON promotions(code)
                WHERE code IS NOT NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_promotions_code",
                table: "promotions");

            migrationBuilder.AddColumn<Guid>(
                name: "order_id",
                table: "promotions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_promotions_order_id",
                table: "promotions",
                column: "order_id");

            migrationBuilder.AddForeignKey(
                name: "fk_promotions_order",
                table: "promotions",
                column: "order_id",
                principalTable: "orders",
                principalColumn: "order_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
