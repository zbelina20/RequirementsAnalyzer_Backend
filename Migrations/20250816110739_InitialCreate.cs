using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RequirementsAnalyzer.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "requirements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    project_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    quality_score = table.Column<int>(type: "integer", nullable: true),
                    analysis_data = table.Column<string>(type: "jsonb", nullable: true),
                    enhancement_data = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requirements", x => x.id);
                    table.ForeignKey(
                        name: "FK_requirements_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "projects",
                columns: new[] { "id", "created_at", "description", "name", "updated_at" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "A sample project for testing", "Sample Project", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "requirements",
                columns: new[] { "id", "analysis_data", "created_at", "enhancement_data", "project_id", "quality_score", "status", "text", "title", "updated_at" },
                values: new object[] { 1, null, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, 1, null, 0, "The system shall provide user authentication functionality", "Sample Requirement", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_projects_name",
                table: "projects",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_requirements_created_at",
                table: "requirements",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_requirements_project_id",
                table: "requirements",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_requirements_status",
                table: "requirements",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "requirements");

            migrationBuilder.DropTable(
                name: "projects");
        }
    }
}
