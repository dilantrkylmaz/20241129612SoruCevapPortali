namespace _20241129612SoruCevapPortalı.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // İlişkiler
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public int UserId { get; set; } // Soruyu soran kişi
        public virtual User User { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }
    }
}