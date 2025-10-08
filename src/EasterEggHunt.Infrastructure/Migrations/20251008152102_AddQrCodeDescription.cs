using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasterEggHunt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCodeDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InternalNote",
                table: "QrCodes",
                newName: "InternalNotes");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "QrCodes",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "QrCodes");

            migrationBuilder.RenameColumn(
                name: "InternalNotes",
                table: "QrCodes",
                newName: "InternalNote");
        }
    }
}
