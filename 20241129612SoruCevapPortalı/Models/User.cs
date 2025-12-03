namespace _20241129612SoruCevapPortalı.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Gerçek hayatta şifrelenmeli ama ödev için düz metin olabilir.
        public string Role { get; set; } // "Admin" veya "Uye"
    }
}