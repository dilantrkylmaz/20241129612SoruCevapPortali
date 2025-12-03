using System.ComponentModel.DataAnnotations;

namespace _20241129612SoruCevapPortalı.Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Cevap")]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Hangi soruya cevap verildi?
        public int QuestionId { get; set; }
        public Question Question { get; set; }

        // Cevabı kim yazdı? (Şimdilik ismen tutabiliriz veya User ile bağlayabiliriz, basit tutalım)
        public string AuthorName { get; set; }
    }
}