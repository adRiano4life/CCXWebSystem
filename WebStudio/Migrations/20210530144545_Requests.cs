using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebStudio.Migrations
{
    public partial class Requests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    DateOfCreate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: true),
                    ExecutorId = table.Column<string>(type: "text", nullable: true),
                    CardId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_AspNetUsers_ExecutorId",
                        column: x => x.ExecutorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Requests_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CardId",
                table: "Requests",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ExecutorId",
                table: "Requests",
                column: "ExecutorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Requests");
        }
    }
}
