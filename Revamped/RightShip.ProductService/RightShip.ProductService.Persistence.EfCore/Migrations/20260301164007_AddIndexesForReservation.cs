using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RightShip.ProductService.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductReservations_ProductId_Status",
                table: "ProductReservations");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReservations_ProductId_Status_ExpiresAt_Quantity",
                table: "ProductReservations",
                columns: new[] { "ProductId", "Status", "ExpiresAt", "Quantity" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReservations_Status_ExpiresAt",
                table: "ProductReservations",
                columns: new[] { "Status", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductReservations_ProductId_Status_ExpiresAt_Quantity",
                table: "ProductReservations");

            migrationBuilder.DropIndex(
                name: "IX_ProductReservations_Status_ExpiresAt",
                table: "ProductReservations");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReservations_ProductId_Status",
                table: "ProductReservations",
                columns: new[] { "ProductId", "Status" });
        }
    }
}
