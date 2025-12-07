using System.ComponentModel.DataAnnotations;

namespace _20241129612SoruCevapPortalı.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        public string Role { get; set; } = "Uye";


        [Required(ErrorMessage = "Ad alanı zorunludur")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "E-Posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon giriniz")]
        public string PhoneNumber { get; set; }

        public string? ProfileImageUrl { get; set; }

  
        public virtual ICollection<Question>? Questions { get; set; }
        public virtual ICollection<Answer>? Answers { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}