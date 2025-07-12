using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YazlabBirSonProje.Migrations
{
    /// <inheritdoc />
    public partial class LogModelChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LogDetails",
                table: "Logs",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "LogDate",
                table: "Logs",
                newName: "Result");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerID",
                table: "Logs",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "CustomerType",
                table: "Logs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductDetails",
                table: "Logs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerType",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "ProductDetails",
                table: "Logs");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Logs",
                newName: "LogDetails");

            migrationBuilder.RenameColumn(
                name: "Result",
                table: "Logs",
                newName: "LogDate");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerID",
                table: "Logs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
