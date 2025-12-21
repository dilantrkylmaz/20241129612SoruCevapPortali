using _20241129612SoruCevapPortalı.Models;

public class QuestionLike
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedDate { get; set; } // Popülerlik hesabı için önemli

    public Question Question { get; set; }
    public User User { get; set; }
}