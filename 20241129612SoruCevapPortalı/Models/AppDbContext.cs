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

            // 1. Cevap - Kullanıcı İlişkisi (Kritik Çözüm)
            modelBuilder.Entity<Answer>()
                .HasOne(x => x.User)
                .WithMany(u => u.Answers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Çakışmayı önlemek için eklendi

            // 2. Soru - Kullanıcı İlişkisi
            modelBuilder.Entity<Question>()
                .HasOne(x => x.User)
                .WithMany(u => u.Questions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // 3. Beğeniler (AnswerLike) İlişkileri
            modelBuilder.Entity<AnswerLike>(entity =>
            {
                entity.HasOne(x => x.Answer)
                    .WithMany(x => x.AnswerLikes)
                    .HasForeignKey(x => x.AnswerId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 4. Beğeniler (QuestionLike) İlişkileri
            modelBuilder.Entity<QuestionLike>(entity =>
            {
                entity.HasOne(x => x.Question)
                    .WithMany(x => x.QuestionLikes)
                    .HasForeignKey(x => x.QuestionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }
}