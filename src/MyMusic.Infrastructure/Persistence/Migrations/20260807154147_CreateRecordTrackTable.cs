using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyMusic.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateRecordTrackTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "record_track",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                record_id = table.Column<int>(type: "integer", nullable: false),
                artist_id = table.Column<int>(type: "integer", nullable: false),
                genre_id = table.Column<int>(type: "integer", nullable: false),
                track_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                record_side = table.Column<string>(
                    type: "character varying(3)",
                    maxLength: 3,
                    nullable: false,
                    defaultValue: "0"),
                track_number = table.Column<short>(type: "smallint", nullable: false),
                information = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_record_track", x => x.id);
                table.ForeignKey(
                    name: "FK_record_track_artist_artist_id",
                    column: x => x.artist_id,
                    principalTable: "artist",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_record_track_genre_genre_id",
                    column: x => x.genre_id,
                    principalTable: "genre",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_record_track_record_record_id",
                    column: x => x.record_id,
                    principalTable: "record",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_record_track_artist_id",
            table: "record_track",
            column: "artist_id");

        migrationBuilder.CreateIndex(
            name: "IX_record_track_genre_id",
            table: "record_track",
            column: "genre_id");

        migrationBuilder.CreateIndex(
            name: "IX_record_track_record_id_record_side_track_number",
            table: "record_track",
            columns: new[] { "record_id", "record_side", "track_number" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "record_track");
    }
}
