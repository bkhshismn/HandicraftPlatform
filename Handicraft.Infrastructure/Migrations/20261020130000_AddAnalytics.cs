using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

/// <summary>
/// جدول‌های آمار بازدید سایت و ویدیوها.
/// چون این جداول به مدل EF نگاشت نشده‌اند، با SQL خام ساخته می‌شوند.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20261020130000_AddAnalytics")]
public partial class AddAnalytics : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS ""PageViews"" (
                ""Id"" uuid NOT NULL,
                ""Path"" text NOT NULL,
                ""UserId"" uuid NULL,
                ""VisitedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_PageViews"" PRIMARY KEY (""Id"")
            );

            CREATE INDEX IF NOT EXISTS ""IX_PageViews_VisitedAt""
                ON ""PageViews"" (""VisitedAt"");
        ");

        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS ""VideoViews"" (
                ""Id"" uuid NOT NULL,
                ""LessonId"" uuid NOT NULL,
                ""EpisodeId"" uuid NOT NULL,
                ""UserId"" uuid NULL,
                ""ViewedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_VideoViews"" PRIMARY KEY (""Id"")
            );

            CREATE INDEX IF NOT EXISTS ""IX_VideoViews_ViewedAt""
                ON ""VideoViews"" (""ViewedAt"");

            CREATE INDEX IF NOT EXISTS ""IX_VideoViews_EpisodeId""
                ON ""VideoViews"" (""EpisodeId"");
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""VideoViews"";");
        migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""PageViews"";");
    }
}
