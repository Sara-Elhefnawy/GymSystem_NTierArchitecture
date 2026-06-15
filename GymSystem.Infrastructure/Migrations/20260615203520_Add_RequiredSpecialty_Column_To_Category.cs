using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RequiredSpecialty_Column_To_Category : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequiredSpecialty",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredSpecialty",
                table: "Categories");
        }
    }
}
