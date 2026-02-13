using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenSpace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDiagnosisCacheAndSymptomDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "address",
                table: "user_address",
                newName: "street_address");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "user_address",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "district",
                table: "user_address",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_default",
                table: "user_address",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "label",
                table: "user_address",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "province",
                table: "user_address",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "user_address",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ward",
                table: "user_address",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "shipping_address",
                table: "orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "recipient_name",
                table: "orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "recipient_phone",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "shipping_address_id",
                table: "orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "diagnosis_cache",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    plant_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    original_description = table.Column<string>(type: "text", nullable: false),
                    normalized_description = table.Column<string>(type: "text", nullable: false),
                    symptoms = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    disease_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ai_response = table.Column<string>(type: "jsonb", nullable: false),
                    confidence_score = table.Column<int>(type: "integer", nullable: true),
                    hit_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP + INTERVAL '90 days'"),
                    cache_ttl_days = table.Column<int>(type: "integer", nullable: false, defaultValue: 90),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    has_image = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("diagnosis_cache_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "symptom_dictionary",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    canonical_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    synonyms = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    english_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("symptom_dictionary_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_cache_active_expires",
                table: "diagnosis_cache",
                columns: new[] { "is_active", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "idx_cache_normalized_desc",
                table: "diagnosis_cache",
                column: "normalized_description");

            migrationBuilder.CreateIndex(
                name: "idx_cache_plant_type",
                table: "diagnosis_cache",
                column: "plant_type");

            migrationBuilder.CreateIndex(
                name: "idx_symptom_canonical_name",
                table: "symptom_dictionary",
                column: "canonical_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_symptom_category",
                table: "symptom_dictionary",
                column: "category");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "diagnosis_cache");

            migrationBuilder.DropTable(
                name: "symptom_dictionary");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "district",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "is_default",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "label",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "province",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "ward",
                table: "user_address");

            migrationBuilder.DropColumn(
                name: "recipient_name",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "recipient_phone",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_address_id",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "street_address",
                table: "user_address",
                newName: "address");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "shipping_address",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
