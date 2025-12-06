namespace _20241129612SoruCevapPortalı.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Şimdilik düz metin
        public string Role { get; set; } = "Uye"; // Varsayılan rol Üye olsun

        // Kullanıcının soruları ve cevapları
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
    }
}