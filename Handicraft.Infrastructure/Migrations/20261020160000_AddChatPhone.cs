using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

/// <summary>
/// افزودن شمارهٔ تماس به پیام‌های چت تا مدیر بتواند با کاربر تماس بگیرد.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20261020160000_AddChatPhone")]
public partial class AddChatPhone : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE ""ChatMessages""
                ADD COLUMN IF NOT EXISTS ""Phone"" text NULL;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE ""ChatMessages"" DROP COLUMN IF EXISTS ""Phone"";
        ");
    }
}
