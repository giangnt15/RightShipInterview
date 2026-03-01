using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RightShip.ProductService.Persistence.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_message",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Topic = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CorrelationId = table.Column<string>(type: "TEXT", nullable: false),
                    Payload = table.Column<string>(type: "longtext", nullable: false),
                    Sent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Processing = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_message", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    QuantityValue = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_CorrelationId",
                table: "outbox_message",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_CreatedAt",
                table: "outbox_message",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_Processing",
                table: "outbox_message",
                column: "Processing");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_Sent",
                table: "outbox_message",
                column: "Sent");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_UpdatedAt",
                table: "outbox_message",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_message");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
