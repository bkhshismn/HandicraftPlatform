using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseEpisodeLessonRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseEpisodes_LessonId",
                table: "CourseEpisodes");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEpisodes_LessonId",
                table: "CourseEpisodes",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseEpisodes_LessonId",
                table: "CourseEpisodes");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEpisodes_LessonId",
                table: "CourseEpisodes",
                column: "LessonId",
                unique: true);
        }
    }
}
