using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AztecTariff.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PDFData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalesAreaID = table.Column<int>(type: "INTEGER", nullable: false),
                    Template = table.Column<string>(type: "TEXT", nullable: false),
                    TempFileName = table.Column<string>(type: "TEXT", nullable: false),
                    IncludeABV = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PDFProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PDFDataId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductID = table.Column<long>(type: "INTEGER", nullable: false),
                    Pricing = table.Column<double>(type: "REAL", nullable: false),
                    IncludedInPdf = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PDFProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pricing",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalesAreaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EstateId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<long>(type: "INTEGER", nullable: false),
                    Price = table.Column<double>(type: "REAL", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pricing", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EstateId = table.Column<int>(type: "INTEGER", nullable: false),
                    EntityCode = table.Column<string>(type: "TEXT", nullable: false),
                    Portion = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdName = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryName = table.Column<string>(type: "TEXT", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ABV = table.Column<double>(type: "REAL", nullable: false),
                    TariffCategory = table.Column<string>(type: "TEXT", nullable: false),
                    ProductTariffName = table.Column<string>(type: "TEXT", nullable: false),
                    Included = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "SalesAreas",
                columns: table => new
                {
                    SalesAreaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EstateId = table.Column<int>(type: "INTEGER", nullable: false),
                    SiteId = table.Column<int>(type: "INTEGER", nullable: false),
                    SiteName = table.Column<string>(type: "TEXT", nullable: false),
                    SAName = table.Column<string>(type: "TEXT", nullable: false),
                    Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Included = table.Column<bool>(type: "INTEGER", nullable: false),
                    TariffName = table.Column<string>(type: "TEXT", nullable: false),
                    FooterMessage = table.Column<string>(type: "TEXT", nullable: true),
                    HeaderMessage = table.Column<string>(type: "TEXT", nullable: true),
                    isEvent = table.Column<bool>(type: "INTEGER", nullable: false),
                    OriginalSalesAreaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesAreas", x => x.SalesAreaId);
                });

            migrationBuilder.CreateTable(
                name: "SummarizedCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SalesAreaID = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    MinPrice = table.Column<double>(type: "REAL", nullable: false),
                    MaxPrice = table.Column<double>(type: "REAL", nullable: false),
                    SummaryDescription = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SummarizedCategories", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PDFData");

            migrationBuilder.DropTable(
                name: "PDFProducts");

            migrationBuilder.DropTable(
                name: "Pricing");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "SalesAreas");

            migrationBuilder.DropTable(
                name: "SummarizedCategories");
        }
    }
}
