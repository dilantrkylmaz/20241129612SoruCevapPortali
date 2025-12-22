using System;
using System.ComponentModel.DataAnnotations;

namespace _20241129612SoruCevapPortalı.Models
{
    public enum ReportStatus { Beklemede, İncelendi, Reddedildi }

    public class Report
    {
        public int Id { get; set; }

        [Required]
        public int ReporterUserId { get; set; } 

        public int? QuestionId { get; set; } 
        public int? AnswerId { get; set; }   

        [Required(ErrorMessage = "Lütfen raporlama nedeninizi belirtin.")]
        public string Reason { get; set; } 

        public ReportStatus Status { get; set; } = ReportStatus.Beklemede;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

       
        public virtual User ReporterUser { get; set; }
        public virtual Question? Question { get; set; }
        public virtual Answer? Answer { get; set; }
    }
}