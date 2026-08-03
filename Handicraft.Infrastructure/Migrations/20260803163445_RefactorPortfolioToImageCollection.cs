using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPortfolioToImageCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioImage_PortfolioItems_PortfolioItemId",
                table: "PortfolioImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PortfolioImage",
                table: "PortfolioImage");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "PortfolioItems");

            migrationBuilder.RenameTable(
                name: "PortfolioImage",
                newName: "PortfolioImages");

            migrationBuilder.RenameIndex(
                name: "IX_PortfolioImage_PortfolioItemId",
                table: "PortfolioImages",
                newName: "IX_PortfolioImages_PortfolioItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PortfolioImages",
                table: "PortfolioImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioImages_PortfolioItems_PortfolioItemId",
                table: "PortfolioImages",
                column: "PortfolioItemId",
                principalTable: "PortfolioItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PortfolioImages_PortfolioItems_PortfolioItemId",
                table: "PortfolioImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PortfolioImages",
                table: "PortfolioImages");

            migrationBuilder.RenameTable(
                name: "PortfolioImages",
                newName: "PortfolioImage");

            migrationBuilder.RenameIndex(
                name: "IX_PortfolioImages_PortfolioItemId",
                table: "PortfolioImage",
                newName: "IX_PortfolioImage_PortfolioItemId");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "PortfolioItems",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PortfolioImage",
                table: "PortfolioImage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PortfolioImage_PortfolioItems_PortfolioItemId",
                table: "PortfolioImage",
                column: "PortfolioItemId",
                principalTable: "PortfolioItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
