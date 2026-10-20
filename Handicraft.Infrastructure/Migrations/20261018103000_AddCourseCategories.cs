using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261018103000_AddCourseCategories")]
public partial class AddCourseCategories : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CourseCategories",
            columns: table => new
            {
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_CourseCategories", x => x.Name));

        migrationBuilder.Sql("INSERT INTO \"CourseCategories\" (\"Name\", \"CreatedAt\") VALUES ('پاپیه‌ماشه | سردیس و پرتره', now()), ('پاپیه‌ماشه | نقش‌برجسته چهره', now()), ('پاپیه‌ماشه | ماسک', now()), ('پاپیه‌ماشه | مجسمه‌های دکوراتیو', now()), ('پاپیه‌ماشه | آموزش و مراحل ساخت', now()) ON CONFLICT (\"Name\") DO NOTHING;");
        migrationBuilder.Sql("INSERT INTO \"CourseCategories\" (\"Name\", \"CreatedAt\") SELECT DISTINCT c.\"Category\", now() FROM \"Courses\" c WHERE trim(c.\"Category\") <> '' ON CONFLICT (\"Name\") DO NOTHING;");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "CourseCategories");
}
