using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace _20241129612SoruCevapPortalı.Models
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<QuestionLike> QuestionLikes { get; set; }
        public DbSet<AnswerLike> AnswerLikes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- AnswerLike İlişkileri ---
            modelBuilder.Entity<AnswerLike>(entity =>
            {
                // Cevap silindiğinde beğeni silme (Çakışmayı önlemek için NoAction)
                entity.HasOne(x => x.Answer)
                    .WithMany(x => x.AnswerLikes)
                    .HasForeignKey(x => x.AnswerId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Kullanıcı silindiğinde beğeni silme
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- QuestionLike İlişkileri ---
            modelBuilder.Entity<QuestionLike>(entity =>
            {
                // Soru silindiğinde beğeni silme
                entity.HasOne(x => x.Question)
                    .WithMany(x => x.QuestionLikes)
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Kullanıcı silindiğinde beğeni silme
                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}