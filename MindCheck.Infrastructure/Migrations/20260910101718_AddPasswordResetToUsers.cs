using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindCheck.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "password_reset_token",
                table: "users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "password_reset_token_expires_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_password_reset_token",
                table: "users",
                column: "password_reset_token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_password_reset_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_token_expires_at",
                table: "users");
        }
    }
}
