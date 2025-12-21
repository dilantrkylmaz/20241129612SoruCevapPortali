namespace _20241129612SoruCevapPortalı.Models
{
    public class AnswerLike
    {
        public int Id { get; set; }
        public int AnswerId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual Answer Answer { get; set; }
        public virtual User User { get; set; }
    }
}