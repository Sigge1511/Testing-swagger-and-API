using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace apiv4.Migrations
{
    /// <inheritdoc />
    public partial class changednamesfordbset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleSet",
                table: "VehicleSet");

            migrationBuilder.RenameTable(
                name: "VehicleSet",
                newName: "BookSet");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookSet",
                table: "BookSet",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BookSet",
                table: "BookSet");

            migrationBuilder.RenameTable(
                name: "BookSet",
                newName: "VehicleSet");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleSet",
                table: "VehicleSet",
                column: "Id");
        }
    }
}
