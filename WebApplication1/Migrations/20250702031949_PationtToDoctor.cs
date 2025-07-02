using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoctorPatientDashboard.Migrations
{
    /// <inheritdoc />
    public partial class PationtToDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_AspNetUsers_DoctorId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_DoctorId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "Patients");

            migrationBuilder.AlterColumn<string>(
                name: "DocID",
                table: "Patients",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_DocID",
                table: "Patients",
                column: "DocID");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_AspNetUsers_DocID",
                table: "Patients",
                column: "DocID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Patients_AspNetUsers_DocID",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_DocID",
                table: "Patients");

            migrationBuilder.AlterColumn<int>(
                name: "DocID",
                table: "Patients",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorId",
                table: "Patients",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_DoctorId",
                table: "Patients",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_AspNetUsers_DoctorId",
                table: "Patients",
                column: "DoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
