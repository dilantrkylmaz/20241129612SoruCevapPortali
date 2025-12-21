using System.ComponentModel.DataAnnotations;

namespace _20241129612SoruCevapPortalı.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cevap boş bırakılamaz.")]
        [Display(Name = "Cevap")]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int QuestionId { get; set; }

        public virtual Question? Question { get; set; }

        public int UserId { get; set; }

        public virtual User? User { get; set; }

        public virtual ICollection<AnswerLike> AnswerLikes { get; set; }
    }
}