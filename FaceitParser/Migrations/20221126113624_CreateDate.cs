using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaceitParser.Migrations
{
    public partial class CreateDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreateDate",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "AspNetUsers");
        }
    }
}
