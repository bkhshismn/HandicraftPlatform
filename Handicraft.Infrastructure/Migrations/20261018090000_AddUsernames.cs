using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Handicraft.Infrastructure.Persistence.Data;

#nullable disable

namespace Handicraft.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261018090000_AddUsernames")]
    public partial class AddUsernames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(name: "Phone", table: "Users", type: "character varying(50)", maxLength: 50, nullable: false, oldClrType: typeof(string), oldType: "character varying(15)", oldMaxLength: 15);
            migrationBuilder.AddColumn<string>(name: "Username", table: "Users", type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "");
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Username\" = \"Phone\" WHERE \"Username\" = '';");
            migrationBuilder.Sql("UPDATE \"Courses\" SET \"Category\" = 'عمومی' WHERE \"Category\" IS NULL OR trim(\"Category\") = '';");
            migrationBuilder.CreateIndex(name: "IX_Users_Username", table: "Users", column: "Username", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Users_Username", table: "Users");
            migrationBuilder.DropColumn(name: "Username", table: "Users");
            migrationBuilder.AlterColumn<string>(name: "Phone", table: "Users", type: "character varying(15)", maxLength: 15, nullable: false, oldClrType: typeof(string), oldType: "character varying(50)", oldMaxLength: 50);
        }
    }
}
