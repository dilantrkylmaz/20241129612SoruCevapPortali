using System.ComponentModel.DataAnnotations;

namespace _20241129612SoruCevapPortalı.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Display(Name = "Konu Başlığı")]
        public string Title { get; set; }

        [Display(Name = "Soru İçeriği")]
        public string Content { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;


        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        public int UserId { get; set; }

        public virtual User? User { get; set; } 

        public virtual ICollection<Answer>? Answers { get; set; }

        public virtual ICollection<QuestionLike> QuestionLikes { get; set; }
    }
}