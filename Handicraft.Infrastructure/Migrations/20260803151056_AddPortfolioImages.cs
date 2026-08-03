using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPortfolioImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PortfolioImage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioImage_PortfolioItems_PortfolioItemId",
                        column: x => x.PortfolioItemId,
                        principalTable: "PortfolioItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioImage_PortfolioItemId",
                table: "PortfolioImage",
                column: "PortfolioItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioImage");
        }
    }
}
