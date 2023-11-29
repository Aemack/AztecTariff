using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AztecTariff.Migrations
{
    /// <inheritdoc />
    public partial class OgSAID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalSalesAreaId",
                table: "SalesAreas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalSalesAreaId",
                table: "SalesAreas");
        }
    }
}
