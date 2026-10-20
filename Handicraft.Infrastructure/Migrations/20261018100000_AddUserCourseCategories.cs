using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261018100000_AddUserCourseCategories")]
public partial class AddUserCourseCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserCourseCategories",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserCourseCategories", x => new { x.UserId, x.Category });
                table.ForeignKey("FK_UserCourseCategories_Users_UserId", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_UserCourseCategories_Category", table: "UserCourseCategories", column: "Category");
        migrationBuilder.Sql("INSERT INTO \"UserCourseCategories\" (\"UserId\", \"Category\") SELECT DISTINCT e.\"UserId\", c.\"Category\" FROM \"CourseEnrollments\" e INNER JOIN \"Courses\" c ON c.\"Id\" = e.\"CourseId\" WHERE e.\"IsActive\" ON CONFLICT DO NOTHING;");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "UserCourseCategories");
}
