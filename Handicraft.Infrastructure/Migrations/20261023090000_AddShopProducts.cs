using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261023090000_AddShopProducts")]
public sealed class AddShopProducts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable(
        name: "ShopProducts",
        columns: table => new
        {
            Id = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
            Name = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
            Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
            ImageUrl = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
            Badge = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            ImagePosition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            Availability = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
            IsPublished = table.Column<bool>(type: "boolean", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table => table.PrimaryKey("PK_ShopProducts", x => x.Id));

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable("ShopProducts");
}
