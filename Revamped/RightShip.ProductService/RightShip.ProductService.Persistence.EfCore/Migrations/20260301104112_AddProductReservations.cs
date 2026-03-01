using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RightShip.ProductService.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductReservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReservations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReservations_ExpiresAt",
                table: "ProductReservations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReservations_ProductId_Status",
                table: "ProductReservations",
                columns: new[] { "ProductId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductReservations");
        }
    }
}
