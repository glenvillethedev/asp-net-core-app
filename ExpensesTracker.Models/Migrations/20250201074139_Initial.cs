using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpensesTracker.Models.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CreateDt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Entries",
                columns: new[] { "Id", "Amount", "Category", "CreateDt", "Date", "Details", "Name", "UpdateDt" },
                values: new object[,]
                {
                    { new Guid("93ab05e5-fc5b-4cc3-b04e-93cb5d7c2f41"), 2345m, 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Details2", "Item3", null },
                    { new Guid("be9a88f1-2e01-4bc5-8e3f-5e1c7a02b4b1"), 123m, 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Details1", "Item2", null },
                    { new Guid("dd87b7e3-b8eb-4b28-b24b-e53eeb5d5427"), 214m, 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Details4", "Item5", null },
                    { new Guid("e6a1f847-0f02-4a4b-a60d-d6235b2e2c6a"), 31m, 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Details5", "Item1", null },
                    { new Guid("fcab3cbb-8e02-4b79-b1f8-11dbf4c0910a"), 591m, 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2023, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Details3", "Item4", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entries");
        }
    }
}
