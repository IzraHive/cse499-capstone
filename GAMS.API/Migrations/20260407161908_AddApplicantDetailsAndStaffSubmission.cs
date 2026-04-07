using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GAMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicantDetailsAndStaffSubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicantContactNumber",
                table: "GrantApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantDateOfBirth",
                table: "GrantApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantGender",
                table: "GrantApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicantTRN",
                table: "GrantApplications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsStaffSubmission",
                table: "GrantApplications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StaffSubmittedForName",
                table: "GrantApplications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicantContactNumber",
                table: "GrantApplications");

            migrationBuilder.DropColumn(
                name: "ApplicantDateOfBirth",
                table: "GrantApplications");

            migrationBuilder.DropColumn(
                name: "ApplicantGender",
                table: "GrantApplications");

            migrationBuilder.DropColumn(
                name: "ApplicantTRN",
                table: "GrantApplications");

            migrationBuilder.DropColumn(
                name: "IsStaffSubmission",
                table: "GrantApplications");

            migrationBuilder.DropColumn(
                name: "StaffSubmittedForName",
                table: "GrantApplications");
        }
    }
}
