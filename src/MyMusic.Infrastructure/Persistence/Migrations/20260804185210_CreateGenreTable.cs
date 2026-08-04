using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyMusic.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateGenreTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "genre",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                genre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_genre", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_genre_user_id_genre",
            table: "genre",
            columns: new[] { "user_id", "genre" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "genre");
    }
}
