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

        // İlişkiler
        public int QuestionId { get; set; }

        // ? İŞARETİ EKLENDİ (Validation hatasını önler)
        public virtual Question? Question { get; set; }

        public int UserId { get; set; }

        // ? İŞARETİ EKLENDİ (Validation hatasını önler)
        public virtual User? User { get; set; }
    }
}