using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Handicraft.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEpisodeToLessonRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseEpisodes_Courses_CourseId",
                table: "CourseEpisodes");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseLessons_CourseEpisodes_EpisodeId",
                table: "CourseLessons");

            migrationBuilder.DropIndex(
                name: "IX_CourseLessons_EpisodeId",
                table: "CourseLessons");

            migrationBuilder.DropColumn(
                name: "EpisodeId",
                table: "CourseLessons");

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "CourseEpisodes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "LessonId",
                table: "CourseEpisodes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CourseEpisodes_LessonId",
                table: "CourseEpisodes",
                column: "LessonId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEpisodes_CourseLessons_LessonId",
                table: "CourseEpisodes",
                column: "LessonId",
                principalTable: "CourseLessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEpisodes_Courses_CourseId",
                table: "CourseEpisodes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseEpisodes_CourseLessons_LessonId",
                table: "CourseEpisodes");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseEpisodes_Courses_CourseId",
                table: "CourseEpisodes");

            migrationBuilder.DropIndex(
                name: "IX_CourseEpisodes_LessonId",
                table: "CourseEpisodes");

            migrationBuilder.DropColumn(
                name: "LessonId",
                table: "CourseEpisodes");

            migrationBuilder.AddColumn<Guid>(
                name: "EpisodeId",
                table: "CourseLessons",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CourseId",
                table: "CourseEpisodes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseLessons_EpisodeId",
                table: "CourseLessons",
                column: "EpisodeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEpisodes_Courses_CourseId",
                table: "CourseEpisodes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseLessons_CourseEpisodes_EpisodeId",
                table: "CourseLessons",
                column: "EpisodeId",
                principalTable: "CourseEpisodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
