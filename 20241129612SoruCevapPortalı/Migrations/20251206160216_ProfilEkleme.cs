using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _20241129612SoruCevapPortalı.Migrations
{
 
    public partial class ProfilEkleme : Migration
    {
   
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

     
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "Users");
        }
    }
}
