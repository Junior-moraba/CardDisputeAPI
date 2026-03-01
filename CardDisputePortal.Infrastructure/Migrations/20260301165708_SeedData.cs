using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CardDisputePortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReasonCode",
                table: "Disputes",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "Name", "PhoneNumber" },
                values: new object[] { new Guid("550e8400-e29b-41d4-a716-446655440000"), new DateTime(2026, 1, 30, 16, 57, 6, 760, DateTimeKind.Utc).AddTicks(3428), null, "Junior Moraba", "0794935197" });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "Amount", "Currency", "Date", "MerchantCategory", "MerchantName", "Reference", "Status", "UserId" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-446655440001"), 1299.99m, "ZAR", new DateTime(2026, 2, 14, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(3865), "Online Retail", "Takealot", "TXN001", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-446655440002"), 65.50m, "ZAR", new DateTime(2026, 2, 15, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(5931), "Food & Beverage", "Seattle Coffee Co", "TXN002", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-446655440003"), 850.00m, "ZAR", new DateTime(2026, 2, 16, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(5939), "Fuel", "Engen", "TXN003", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-446655440004"), 89.90m, "ZAR", new DateTime(2026, 2, 17, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(5945), "Food & Beverage", "Steers", "TXN004", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-446655440005"), 2499.99m, "ZAR", new DateTime(2026, 2, 18, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(5951), "Retail", "Mr Price Home", "TXN005", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-446655440006"), 125.50m, "ZAR", new DateTime(2026, 2, 19, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(5956), "Food & Beverage", "KFC", "TXN006", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-446655440007"), 4999.99m, "ZAR", new DateTime(2026, 2, 20, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(5991), "Electronics", "Incredible Connection", "TXN007", 2, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-446655440008"), 567.89m, "ZAR", new DateTime(2026, 2, 21, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(5996), "Groceries", "Pick n Pay", "TXN008", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-446655440009"), 199.00m, "ZAR", new DateTime(2026, 2, 22, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(6026), "Entertainment", "Netflix", "TXN009", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000a"), 1899.99m, "ZAR", new DateTime(2026, 2, 23, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(6032), "Retail", "Sportsmans Warehouse", "TXN010", 2, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000b"), 234.50m, "ZAR", new DateTime(2026, 2, 24, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(6038), "Food & Beverage", "Nandos", "TXN011", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000c"), 178.99m, "ZAR", new DateTime(2026, 2, 25, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(6043), "Health & Beauty", "Clicks", "TXN012", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000d"), 725.00m, "ZAR", new DateTime(2026, 2, 26, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(6049), "Fuel", "Shell", "TXN013", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000e"), 899.99m, "ZAR", new DateTime(2026, 2, 27, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(6054), "Entertainment", "Game", "TXN014", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("550e8400-e29b-41d4-a716-44665544000f"), 189.50m, "ZAR", new DateTime(2026, 2, 28, 16, 57, 6, 762, DateTimeKind.Utc).AddTicks(6060), "Transport", "Uber", "TXN015", 0, new Guid("550e8400-e29b-41d4-a716-446655440000") }
                });

            migrationBuilder.InsertData(
                table: "Disputes",
                columns: new[] { "Id", "Details", "EstimatedResolutionDate", "EvidenceAttached", "ReasonCode", "Status", "SubmittedAt", "TransactionId", "UserId" },
                values: new object[,]
                {
                    { new Guid("660e8400-e29b-41d4-a716-446655440001"), "Did not make this purchase", new DateTime(2026, 3, 6, 16, 57, 6, 763, DateTimeKind.Utc).AddTicks(1759), false, 0, 0, new DateTime(2026, 2, 19, 16, 57, 6, 763, DateTimeKind.Utc).AddTicks(1356), new Guid("550e8400-e29b-41d4-a716-446655440001"), new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("660e8400-e29b-41d4-a716-446655440002"), "Item never delivered", new DateTime(2026, 3, 11, 16, 57, 6, 763, DateTimeKind.Utc).AddTicks(2140), false, 3, 1, new DateTime(2026, 2, 24, 16, 57, 6, 763, DateTimeKind.Utc).AddTicks(2138), new Guid("550e8400-e29b-41d4-a716-446655440007"), new Guid("550e8400-e29b-41d4-a716-446655440000") },
                    { new Guid("660e8400-e29b-41d4-a716-446655440003"), "Charged twice for same item", new DateTime(2026, 2, 28, 16, 57, 6, 763, DateTimeKind.Utc).AddTicks(2152), false, 1, 2, new DateTime(2026, 2, 27, 16, 57, 6, 763, DateTimeKind.Utc).AddTicks(2150), new Guid("550e8400-e29b-41d4-a716-44665544000a"), new Guid("550e8400-e29b-41d4-a716-446655440000") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Disputes",
                keyColumn: "Id",
                keyValue: new Guid("660e8400-e29b-41d4-a716-446655440001"));

            migrationBuilder.DeleteData(
                table: "Disputes",
                keyColumn: "Id",
                keyValue: new Guid("660e8400-e29b-41d4-a716-446655440002"));

            migrationBuilder.DeleteData(
                table: "Disputes",
                keyColumn: "Id",
                keyValue: new Guid("660e8400-e29b-41d4-a716-446655440003"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440002"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440003"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440004"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440005"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440006"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440008"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440009"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000b"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000c"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000d"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000e"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000f"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440001"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440007"));

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-44665544000a"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440000"));

            migrationBuilder.AlterColumn<string>(
                name: "ReasonCode",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
