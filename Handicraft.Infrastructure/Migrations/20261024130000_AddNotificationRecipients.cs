using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261024130000_AddNotificationRecipients")]
public partial class AddNotificationRecipients : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(@"
        ALTER TABLE ""AdminNotifications"" ADD COLUMN IF NOT EXISTS ""RecipientUserId"" uuid NULL;
        CREATE INDEX IF NOT EXISTS ""IX_AdminNotifications_RecipientUserId_IsRead_CreatedAt""
            ON ""AdminNotifications"" (""RecipientUserId"", ""IsRead"", ""CreatedAt"");
    ");

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(@"
        DROP INDEX IF EXISTS ""IX_AdminNotifications_RecipientUserId_IsRead_CreatedAt"";
        ALTER TABLE ""AdminNotifications"" DROP COLUMN IF EXISTS ""RecipientUserId"";
    ");
}
