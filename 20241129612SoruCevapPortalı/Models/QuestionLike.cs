namespace _20241129612SoruCevapPortalı.Models
{
    public class QuestionLike
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public virtual Question Question { get; set; }
        public virtual User User { get; set; }
    }
}