using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

/// <summary>
/// پیام‌های چت بین بازدیدکننده و مدیر سایت.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20261020150000_AddChatMessages")]
public partial class AddChatMessages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS ""ChatMessages"" (
                ""Id"" uuid NOT NULL,
                ""ConversationId"" uuid NOT NULL,
                ""UserId"" uuid NULL,
                ""SenderRole"" text NOT NULL,
                ""SenderName"" text NOT NULL,
                ""Body"" text NOT NULL,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                CONSTRAINT ""PK_ChatMessages"" PRIMARY KEY (""Id"")
            );

            CREATE INDEX IF NOT EXISTS ""IX_ChatMessages_Conversation""
                ON ""ChatMessages"" (""ConversationId"", ""CreatedAt"");
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP TABLE IF EXISTS ""ChatMessages"";");
    }
}
