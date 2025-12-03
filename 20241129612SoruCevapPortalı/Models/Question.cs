using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _20241129612SoruCevapPortalı.Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Soru Başlığı")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Soru İçeriği")]
        public string Content { get; set; }

        [Display(Name = "Tarih")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Kategori İlişkisi
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Cevaplar
        public ICollection<Answer> Answers { get; set; }
    }
}