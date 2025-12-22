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


            modelBuilder.Entity<Question>()
                .ToTable(tb => tb.HasTrigger("TR_FullDeleteQuestion"));

            modelBuilder.Entity<Answer>()
                .ToTable(tb => tb.HasTrigger("TR_FullDeleteAnswer"));

            modelBuilder.Entity<User>()
                .ToTable(tb => tb.HasTrigger("TR_FullDeleteUser"));

            modelBuilder.Entity<Category>()
                .ToTable(tb => tb.HasTrigger("TR_FullDeleteCategory"));

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
    }
}