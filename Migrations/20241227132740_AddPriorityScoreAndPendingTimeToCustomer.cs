using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YazlabBirSonProje.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityScoreAndPendingTimeToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "PriorityScore",
                table: "Customers",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<double>(
                name: "PendingTime",
                table: "Customers",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingTime",
                table: "Customers");

            migrationBuilder.AlterColumn<int>(
                name: "PriorityScore",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");
        }
    }
}
