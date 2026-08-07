using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyMusic.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateLabelTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "label",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                label_name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                country_id = table.Column<int>(type: "integer", nullable: false),
                information = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_label", x => x.id);
                table.ForeignKey(
                    name: "FK_label_country_country_id",
                    column: x => x.country_id,
                    principalTable: "country",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_label_country_id",
            table: "label",
            column: "country_id");

        migrationBuilder.CreateIndex(
            name: "IX_label_user_id_label_name",
            table: "label",
            columns: new[] { "user_id", "label_name" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "label");
    }
}
