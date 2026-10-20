using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResumeProgressPerEpisode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CourseLessonProgresses_UserId",
                table: "CourseLessonProgresses");

            migrationBuilder.AddColumn<Guid>(
                name: "EpisodeId",
                table: "CourseLessonProgresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessonProgresses_EpisodeId",
                table: "CourseLessonProgresses",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessonProgresses_UserId_LessonId_EpisodeId",
                table: "CourseLessonProgresses",
                columns: new[] { "UserId", "LessonId", "EpisodeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CourseLessonProgresses_CourseEpisodes_EpisodeId",
                table: "CourseLessonProgresses",
                column: "EpisodeId",
                principalTable: "CourseEpisodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseLessonProgresses_CourseEpisodes_EpisodeId",
                table: "CourseLessonProgresses");

            migrationBuilder.DropIndex(
                name: "IX_CourseLessonProgresses_EpisodeId",
                table: "CourseLessonProgresses");

            migrationBuilder.DropIndex(
                name: "IX_CourseLessonProgresses_UserId_LessonId_EpisodeId",
                table: "CourseLessonProgresses");

            migrationBuilder.DropColumn(
                name: "EpisodeId",
                table: "CourseLessonProgresses");

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessonProgresses_UserId",
                table: "CourseLessonProgresses",
                column: "UserId");
        }
    }
}
