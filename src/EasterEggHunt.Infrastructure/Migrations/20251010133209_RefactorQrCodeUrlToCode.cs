using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasterEggHunt.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorQrCodeUrlToCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the new Code column first (nullable initially)
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "QrCodes",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            // Extract the code part from existing UniqueUrl values
            // UniqueUrl format: https://easteregghunt.local/qr/{code}
            migrationBuilder.Sql(@"
                UPDATE QrCodes 
                SET Code = SUBSTR(UniqueUrl, LENGTH('https://easteregghunt.local/qr/') + 1)
                WHERE UniqueUrl IS NOT NULL AND UniqueUrl != ''
            ");

            // Make Code column non-nullable
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "QrCodes",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            // Drop the old UniqueUrl index and column
            migrationBuilder.DropIndex(
                name: "IX_QrCodes_UniqueUrl",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "UniqueUrl",
                table: "QrCodes");

            // Create the new Code index
            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_Code",
                table: "QrCodes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new Code index and column
            migrationBuilder.DropIndex(
                name: "IX_QrCodes_Code",
                table: "QrCodes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "QrCodes");

            // Add back the UniqueUrl column
            migrationBuilder.AddColumn<string>(
                name: "UniqueUrl",
                table: "QrCodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Reconstruct UniqueUrl values from Code
            // Note: This is a simplified reconstruction - in practice you might want to preserve the original domain
            migrationBuilder.Sql(@"
                UPDATE QrCodes 
                SET UniqueUrl = 'https://easteregghunt.local/qr/' || Code
                WHERE Code IS NOT NULL AND Code != ''
            ");

            // Create the old UniqueUrl index
            migrationBuilder.CreateIndex(
                name: "IX_QrCodes_UniqueUrl",
                table: "QrCodes",
                column: "UniqueUrl",
                unique: true);
        }
    }
}
