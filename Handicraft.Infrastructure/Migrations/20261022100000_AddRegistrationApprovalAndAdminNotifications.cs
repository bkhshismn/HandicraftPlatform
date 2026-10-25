using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261021150000_AddRegistrationApprovalAndAdminNotifications")]
public partial class AddRegistrationApprovalAndAdminNotifications : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE ""Users""
                ADD COLUMN IF NOT EXISTS ""IsApproved"" boolean NOT NULL DEFAULT TRUE;

            CREATE TABLE IF NOT EXISTS ""AdminNotifications"" (
                ""Id"" uuid NOT NULL PRIMARY KEY,
                ""Title"" varchar(120) NOT NULL,
                ""Message"" varchar(500) NOT NULL,
                ""TargetUrl"" varchar(300) NOT NULL,
                ""IsRead"" boolean NOT NULL DEFAULT FALSE,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""UpdatedAt"" timestamp with time zone NULL
            );

            CREATE INDEX IF NOT EXISTS ""IX_AdminNotifications_IsRead_CreatedAt""
                ON ""AdminNotifications"" (""IsRead"", ""CreatedAt"");
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DROP TABLE IF EXISTS ""AdminNotifications"";
            ALTER TABLE ""Users"" DROP COLUMN IF EXISTS ""IsApproved"";
        ");
    }
}
