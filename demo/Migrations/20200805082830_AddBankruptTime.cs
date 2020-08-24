using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyDemo.Migrations
{
    public partial class AddBankruptTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BankruptTime",
                table: "Companies",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankruptTime",
                table: "Companies");
        }
    }
}
