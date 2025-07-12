using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YazlabBirSonProje.Migrations
{
    /// <inheritdoc />
    public partial class CustomerModelChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriorityScore",
                table: "Customers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriorityScore",
                table: "Customers");
        }
    }
}
