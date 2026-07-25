using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkLessonToEpisode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoPath",
                table: "CourseLessons");

            migrationBuilder.AddColumn<Guid>(
                name: "EpisodeId",
                table: "CourseLessons",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_EpisodeId",
                table: "CourseLessons",
                column: "EpisodeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseLessons_CourseEpisodes_EpisodeId",
                table: "CourseLessons",
                column: "EpisodeId",
                principalTable: "CourseEpisodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseLessons_CourseEpisodes_EpisodeId",
                table: "CourseLessons");

            migrationBuilder.DropIndex(
                name: "IX_CourseLessons_EpisodeId",
                table: "CourseLessons");

            migrationBuilder.DropColumn(
                name: "EpisodeId",
                table: "CourseLessons");

            migrationBuilder.AddColumn<string>(
                name: "VideoPath",
                table: "CourseLessons",
                type: "text",
                nullable: true);
        }
    }
}
