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
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- TETİKLEYİCİLERİ EF CORE'A BİLDİRME (KRİTİK FİX) ---

            // Soru tablosu için tetikleyiciyi bildir
            modelBuilder.Entity<Question>()
                .ToTable(tb => tb.HasTrigger("TR_FullDeleteQuestion"));

            // Cevap tablosu için tetikleyiciyi bildir
            modelBuilder.Entity<Answer>()
                .ToTable(tb => tb.HasTrigger("TR_FullDeleteAnswer"));

            // Kullanıcı tablosu için tetikleyiciyi bildir (Identity kullanıyorsan)
            modelBuilder.Entity<User>()
                .ToTable(tb => tb.HasTrigger("TR_FullDeleteUser"));

            // Kategori tablosu için tetikleyiciyi bildir
            modelBuilder.Entity<Category>()
                .ToTable(tb => tb.HasTrigger("TR_FullDeleteCategory"));

            // Daha önce yazdığımız NoAction döngüsü burada kalmaya devam etsin
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }
}