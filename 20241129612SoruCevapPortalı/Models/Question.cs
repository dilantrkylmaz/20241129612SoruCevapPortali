using System.ComponentModel.DataAnnotations; // Bunu eklemeyi unutma

namespace _20241129612SoruCevapPortalı.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Display(Name = "Konu Başlığı")]
        public string Title { get; set; }

        [Display(Name = "Soru İçeriği")]
        public string Content { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // --- İLİŞKİLER (Soru İşaretlerini Ekledik) ---

        public int CategoryId { get; set; }

        // ? işareti, "Validation yaparken bu nesne boş olabilir" demektir.
        public virtual Category? Category { get; set; }

        public int UserId { get; set; }

        public virtual User? User { get; set; } // Soru işareti eklendi

        public virtual ICollection<Answer>? Answers { get; set; } // Soru işareti eklendi
    }
}