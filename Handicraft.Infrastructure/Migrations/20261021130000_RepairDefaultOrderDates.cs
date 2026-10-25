using Handicraft.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261021130000_RepairDefaultOrderDates")]
public partial class RepairDefaultOrderDates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            UPDATE ""Orders""
            SET ""CreatedAt"" = NOW()
            WHERE ""CreatedAt"" <= TIMESTAMPTZ '0001-01-02 00:00:00+00';
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
