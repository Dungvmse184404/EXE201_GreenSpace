using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenSpace.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDiseaseKnowledgeBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "diseases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    disease_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    english_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    causes = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    immediate_actions = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    long_term_care = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    prevention_tips = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    watering_advice = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    lighting_advice = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fertilizing_advice = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_urls = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    product_keywords = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    notes = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("diseases_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plant_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    common_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    scientific_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    family = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("plant_types_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "disease_symptoms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    disease_id = table.Column<Guid>(type: "uuid", nullable: false),
                    symptom_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 1.0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("disease_symptoms_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_disease_symptoms_disease",
                        column: x => x.disease_id,
                        principalTable: "diseases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_disease_symptoms_symptom",
                        column: x => x.symptom_id,
                        principalTable: "symptom_dictionary",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plant_type_diseases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    plant_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    disease_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prevalence = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("plant_type_diseases_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_plant_type_diseases_disease",
                        column: x => x.disease_id,
                        principalTable: "diseases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_plant_type_diseases_plant_type",
                        column: x => x.plant_type_id,
                        principalTable: "plant_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_disease_symptoms_disease",
                table: "disease_symptoms",
                column: "disease_id");

            migrationBuilder.CreateIndex(
                name: "idx_disease_symptoms_symptom",
                table: "disease_symptoms",
                column: "symptom_id");

            migrationBuilder.CreateIndex(
                name: "idx_disease_name",
                table: "diseases",
                column: "disease_name");

            migrationBuilder.CreateIndex(
                name: "idx_plant_type_diseases_disease",
                table: "plant_type_diseases",
                column: "disease_id");

            migrationBuilder.CreateIndex(
                name: "idx_plant_type_diseases_plant_type",
                table: "plant_type_diseases",
                column: "plant_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_plant_type_common_name",
                table: "plant_types",
                column: "common_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "disease_symptoms");

            migrationBuilder.DropTable(
                name: "plant_type_diseases");

            migrationBuilder.DropTable(
                name: "diseases");

            migrationBuilder.DropTable(
                name: "plant_types");
        }
    }
}
