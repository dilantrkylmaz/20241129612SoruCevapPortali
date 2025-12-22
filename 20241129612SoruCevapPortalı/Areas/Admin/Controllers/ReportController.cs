using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _20241129612SoruCevapPortalı.Models;
using System.Linq;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var reports = _context.Reports
                .Include(r => r.ReporterUser)
                .Include(r => r.Question)
                .Include(r => r.Answer)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, ReportStatus status)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                report.Status = status;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}