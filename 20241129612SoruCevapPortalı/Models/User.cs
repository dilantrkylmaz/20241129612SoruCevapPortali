using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _20241129612SoruCevapPortalı.Models
{
    public class User : IdentityUser<int>
    {
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        public string LastName { get; set; }

        public string? Role { get; set; } = "Uye";

        public string? ProfileImageUrl { get; set; }

        [NotMapped]
        public string? Password { get; set; }

        public virtual ICollection<Question>? Questions { get; set; }
        public virtual ICollection<Answer>? Answers { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}