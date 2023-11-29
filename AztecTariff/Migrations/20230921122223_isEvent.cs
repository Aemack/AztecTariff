using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AztecTariff.Migrations
{
    /// <inheritdoc />
    public partial class isEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isEvent",
                table: "SalesAreas",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isEvent",
                table: "SalesAreas");
        }
    }
}
