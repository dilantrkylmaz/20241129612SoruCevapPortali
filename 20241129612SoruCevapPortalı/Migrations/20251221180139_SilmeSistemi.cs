using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _20241129612SoruCevapPortalı.Migrations
{
    public partial class silmeyönetim : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. SORU SİLİNDİĞİNDE TEMİZLİK
            migrationBuilder.Sql(@"
                CREATE TRIGGER TR_FullDeleteQuestion ON Questions INSTEAD OF DELETE AS BEGIN
                    SET NOCOUNT ON;
                    DECLARE @QId INT; SELECT @QId = Id FROM deleted;
                    DELETE FROM AnswerLikes WHERE AnswerId IN (SELECT Id FROM Answers WHERE QuestionId = @QId);
                    DELETE FROM Reports WHERE AnswerId IN (SELECT Id FROM Answers WHERE QuestionId = @QId);
                    DELETE FROM QuestionLikes WHERE QuestionId = @QId;
                    DELETE FROM Reports WHERE QuestionId = @QId;
                    DELETE FROM Answers WHERE QuestionId = @QId;
                    DELETE FROM Questions WHERE Id = @QId;
                END");

            // 2. CEVAP SİLİNDİĞİNDE TEMİZLİK (Hata veren yer burasıydı, eklendi!)
            migrationBuilder.Sql(@"
                CREATE TRIGGER TR_FullDeleteAnswer ON Answers INSTEAD OF DELETE AS BEGIN
                    SET NOCOUNT ON;
                    DECLARE @AId INT; SELECT @AId = Id FROM deleted;
                    DELETE FROM AnswerLikes WHERE AnswerId = @AId;
                    DELETE FROM Reports WHERE AnswerId = @AId;
                    DELETE FROM Answers WHERE Id = @AId;
                END");

            // 3. KULLANICI SİLİNDİĞİNDE TEMİZLİK
            migrationBuilder.Sql(@"
                CREATE TRIGGER TR_FullDeleteUser ON AspNetUsers INSTEAD OF DELETE AS BEGIN
                    SET NOCOUNT ON;
                    DECLARE @UId INT; SELECT @UId = Id FROM deleted;
                    DELETE FROM AnswerLikes WHERE UserId = @UId OR AnswerId IN (SELECT Id FROM Answers WHERE UserId = @UId);
                    DELETE FROM QuestionLikes WHERE UserId = @UId OR QuestionId IN (SELECT Id FROM Questions WHERE UserId = @UId);
                    DELETE FROM Reports WHERE ReporterUserId = @UId OR QuestionId IN (SELECT Id FROM Questions WHERE UserId = @UId) OR AnswerId IN (SELECT Id FROM Answers WHERE UserId = @UId);
                    DELETE FROM Answers WHERE UserId = @UId OR QuestionId IN (SELECT Id FROM Questions WHERE UserId = @UId);
                    DELETE FROM Questions WHERE UserId = @UId;
                    DELETE FROM AspNetUsers WHERE Id = @UId;
                END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_FullDeleteQuestion");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_FullDeleteAnswer");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_FullDeleteUser");
        }
    }
}