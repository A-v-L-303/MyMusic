using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyMusic.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCollectionNumberToRecord : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "collection_number",
            table: "record",
            type: "integer",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE record r
            SET collection_number = sub.rn
            FROM (SELECT id, ROW_NUMBER() OVER (PARTITION BY user_id ORDER BY id) AS rn FROM record) sub
            WHERE r.id = sub.id;
            """);

        migrationBuilder.AlterColumn<int>(
            name: "collection_number",
            table: "record",
            type: "integer",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_record_user_id_collection_number",
            table: "record",
            columns: new[] { "user_id", "collection_number" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_record_user_id_collection_number",
            table: "record");

        migrationBuilder.DropColumn(
            name: "collection_number",
            table: "record");
    }
}
