using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _20241129612SoruCevapPortalı.Migrations
{
    /// <inheritdoc />
    public partial class KategoriSilmeSistemi2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE TRIGGER TR_FullDeleteCategory ON Categories INSTEAD OF DELETE AS BEGIN
            SET NOCOUNT ON;
            DECLARE @CatId INT;
            SELECT @CatId = Id FROM deleted;

            -- 1. Bu kategorideki soruların cevaplarının beğenilerini ve raporlarını sil
            DELETE FROM AnswerLikes WHERE AnswerId IN (SELECT Id FROM Answers WHERE QuestionId IN (SELECT Id FROM Questions WHERE CategoryId = @CatId));
            DELETE FROM Reports WHERE AnswerId IN (SELECT Id FROM Answers WHERE QuestionId IN (SELECT Id FROM Questions WHERE CategoryId = @CatId));

            -- 2. Bu kategorideki soruların cevaplarını sil
            DELETE FROM Answers WHERE QuestionId IN (SELECT Id FROM Questions WHERE CategoryId = @CatId);

            -- 3. Bu kategorideki soruların beğenilerini ve raporlarını sil
            DELETE FROM QuestionLikes WHERE QuestionId IN (SELECT Id FROM Questions WHERE CategoryId = @CatId);
            DELETE FROM Reports WHERE QuestionId IN (SELECT Id FROM Questions WHERE CategoryId = @CatId);

            -- 4. Bu kategorideki soruları sil
            DELETE FROM Questions WHERE CategoryId = @CatId;

            -- 5. En son kategorinin kendisini sil
            DELETE FROM Categories WHERE Id = @CatId;
        END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_FullDeleteCategory");
        }
    }
}
