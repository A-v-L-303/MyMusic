using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyMusic.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class CreateRecordTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("Npgsql:Enum:record_condition", "Mint,NM,VG+,VG,G+,G,P")
            .Annotation(
                "Npgsql:Enum:record_format",
                "Album,MaxiSingle,Single,EP,Compilation,CD-Album,CD-MaxiSingle,CD-Single,CD-EP,CD-Compilation");

        migrationBuilder.CreateTable(
            name: "record",
            columns: table => new
            {
                id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation(
                        "Npgsql:ValueGenerationStrategy",
                        NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                label_id = table.Column<int>(type: "integer", nullable: false),
                artist_id = table.Column<int>(type: "integer", nullable: true),
                album_cover = table.Column<byte[]>(type: "bytea", nullable: true),
                format = table.Column<int>(type: "record_format", nullable: false),
                album_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                release_year = table.Column<short>(type: "smallint", nullable: false),
                condition = table.Column<int>(
                    type: "record_condition",
                    nullable: false,
                    defaultValueSql: "'VG'::record_condition"),
                information = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_record", x => x.id);
                table.ForeignKey(
                    name: "FK_record_artist_artist_id",
                    column: x => x.artist_id,
                    principalTable: "artist",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_record_label_label_id",
                    column: x => x.label_id,
                    principalTable: "label",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_record_artist_id",
            table: "record",
            column: "artist_id");

        migrationBuilder.CreateIndex(
            name: "IX_record_label_id",
            table: "record",
            column: "label_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "record");

        migrationBuilder.AlterDatabase()
            .OldAnnotation("Npgsql:Enum:record_condition", "Mint,NM,VG+,VG,G+,G,P")
            .OldAnnotation(
                "Npgsql:Enum:record_format",
                "Album,MaxiSingle,Single,EP,Compilation,CD-Album,CD-MaxiSingle,CD-Single,CD-EP,CD-Compilation");
    }
}
