using System;
using System.ComponentModel.DataAnnotations;

namespace _20241129612SoruCevapPortalı.Models
{
    public enum ReportStatus { Beklemede, İncelendi, Reddedildi }

    public class Report
    {
        public int Id { get; set; }

        [Required]
        public int ReporterUserId { get; set; } // Raporlayan

        public int? QuestionId { get; set; } // Eğer bir soru raporlanıyorsa
        public int? AnswerId { get; set; }   // Eğer bir cevap raporlanıyorsa

        [Required(ErrorMessage = "Lütfen raporlama nedeninizi belirtin.")]
        public string Reason { get; set; } // Rapor Nedeni

        public ReportStatus Status { get; set; } = ReportStatus.Beklemede;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual User ReporterUser { get; set; }
        public virtual Question? Question { get; set; }
        public virtual Answer? Answer { get; set; }
    }
}