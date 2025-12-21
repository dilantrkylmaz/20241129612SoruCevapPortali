using _20241129612SoruCevapPortalı.Hubs;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Answer> _answerRepo;
        private readonly IHubContext<PortalHub> _hubContext;
        private readonly AppDbContext _context;

        public QuestionController(IRepository<Question> questionRepo, IRepository<Category> categoryRepo, IRepository<Answer> answerRepo, IHubContext<PortalHub> hubContext, AppDbContext context)
        {
            _questionRepo = questionRepo;
            _categoryRepo = categoryRepo;
            _answerRepo = answerRepo;
            _hubContext = hubContext;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name");
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(Question p)
        {
            if (ModelState.IsValid)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    var userId = int.Parse(userIdStr);
                    p.UserId = userId;
                    p.CreatedDate = DateTime.Now;

                    _questionRepo.Add(p);

                    // KULLANICI ADI FIX: Veritabanından gerçek UserName'i alıyoruz
                    var user = _context.Users.Find(userId);
                    string realUserName = user?.UserName ?? "Anonim";

                    // SignalR Bildirimi (Gerçek isimle gidiyor)
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", realUserName, p.Title);

                    return RedirectToAction("Index", "Home");
                }
            }

            // --- BURASI KRİTİK: Eğer hata varsa sayfaya geri dön ve kategorileri tekrar yükle ---
            // Aksi takdirde "DropdownList" boş olduğu için sayfa hata verir ve buton çalışmaz.
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name", p.CategoryId);
            return View(p);
        }

        public IActionResult Details(int id)
        {
            var question = _questionRepo.Get(x => x.Id == id,
                "User", "Category", "Answers", "Answers.User", "QuestionLikes", "Answers.AnswerLikes");

            if (question == null) return NotFound();

            // POPÜLER SORULAR FIX: Detay sayfasında sağ tarafın dolması için
            DateTime filterDate = DateTime.Now.AddMonths(-1);
            ViewBag.PopularQuestions = _questionRepo.GetAll(x => x.QuestionLikes)
                .Where(x => x.CreatedDate >= filterDate)
                .OrderByDescending(x => x.QuestionLikes.Count)
                .Take(5).ToList();

            return View(question);
        }

        // --- BEĞENİ VE AJAX METOTLARI ---

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LikeQuestionAjax(int questionId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingLike = await _context.QuestionLikes.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.UserId == userId);
            if (existingLike == null) _context.QuestionLikes.Add(new QuestionLike { QuestionId = questionId, UserId = userId, CreatedDate = DateTime.Now });
            else _context.QuestionLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
            var count = await _context.QuestionLikes.CountAsync(x => x.QuestionId == questionId);
            return Json(new { success = true, count = count });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LikeAnswerAjax(int answerId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingLike = await _context.AnswerLikes.FirstOrDefaultAsync(x => x.AnswerId == answerId && x.UserId == userId);
            if (existingLike == null) _context.AnswerLikes.Add(new AnswerLike { AnswerId = answerId, UserId = userId, CreatedDate = DateTime.Now });
            else _context.AnswerLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
            var count = await _context.AnswerLikes.CountAsync(x => x.AnswerId == answerId);
            return Json(new { success = true, count = count });
        }

        [HttpPost]
        public IActionResult CreateAnswerAjax(string Content, int QuestionId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Lütfen önce giriş yapınız." });
            var userId = int.Parse(userIdStr);
            Answer answer = new Answer { Content = Content, QuestionId = QuestionId, UserId = userId, CreatedDate = DateTime.Now };
            _answerRepo.Add(answer);
            var user = _context.Users.Find(userId);
            return Json(new { success = true, UserName = user?.UserName ?? "Anonim", content = answer.Content, date = answer.CreatedDate.ToString("dd.MM.yyyy HH:mm") });
        }

        // --- DÜZENLEME VE RAPORLAMA ---

        [Authorize]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var question = _questionRepo.GetById(id);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (question == null || (question.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("MainAdmin"))) return Forbid();
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name", question.CategoryId);
            return View(question);
        }

        [Authorize]
        [HttpPost]
        public IActionResult Edit(Question model)
        {
            var question = _questionRepo.GetById(model.Id);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (question == null || (question.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("MainAdmin"))) return Forbid();
            if (ModelState.IsValid)
            {
                question.Title = model.Title;
                question.Content = model.Content;
                question.CategoryId = model.CategoryId;
                _questionRepo.Update(question);
                return RedirectToAction("Details", new { id = question.Id });
            }
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name", model.CategoryId);
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReportAjax(int? questionId, int? answerId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return Json(new { success = false, message = "Neden belirtiniz." });
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var report = new Report { ReporterUserId = userId, QuestionId = questionId, AnswerId = answerId, Reason = reason, CreatedDate = DateTime.Now };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Bildirim iletildi." });
        }

        [Authorize(Roles = "Admin,MainAdmin")]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _questionRepo.GetById(id);
            if (question != null) _questionRepo.Delete(question);
            return RedirectToAction("Index", "Home");
        }
    }
}