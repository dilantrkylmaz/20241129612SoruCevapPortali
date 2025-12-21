using _20241129612SoruCevapPortalı.Hubs;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore; // FirstOrDefaultAsync vb. için gerekli
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
                    p.UserId = int.Parse(userIdStr);
                    p.CreatedDate = DateTime.Now;
                    _questionRepo.Add(p);
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", User.Identity.Name, p.Title);
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name");
            return View(p);
        }

        public IActionResult Details(int id)
        {
            // DÜZELTİLDİ: Beğeniler (QuestionLikes ve AnswerLikes) veritabanından çekiliyor
            var question = _questionRepo.Get(x => x.Id == id,
                "User", "Category", "Answers", "Answers.User", "QuestionLikes", "Answers.AnswerLikes");

            if (question == null) return NotFound();
            return View(question);
        }

        // --- BEĞENİ SİSTEMİ (AJAX UYUMLU) ---

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LikeQuestionAjax(int questionId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false });

            var userId = int.Parse(userIdStr);
            var existingLike = await _context.QuestionLikes
                .FirstOrDefaultAsync(x => x.QuestionId == questionId && x.UserId == userId);

            bool isLiked;
            if (existingLike == null)
            {
                _context.QuestionLikes.Add(new QuestionLike { QuestionId = questionId, UserId = userId, CreatedDate = DateTime.Now });
                isLiked = true;
            }
            else
            {
                _context.QuestionLikes.Remove(existingLike);
                isLiked = false;
            }

            await _context.SaveChangesAsync();
            var count = await _context.QuestionLikes.CountAsync(x => x.QuestionId == questionId);
            return Json(new { success = true, isLiked = isLiked, count = count });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LikeAnswerAjax(int answerId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false });

            var userId = int.Parse(userIdStr);
            var existingLike = await _context.AnswerLikes
                .FirstOrDefaultAsync(x => x.AnswerId == answerId && x.UserId == userId);

            bool isLiked;
            if (existingLike == null)
            {
                _context.AnswerLikes.Add(new AnswerLike { AnswerId = answerId, UserId = userId, CreatedDate = DateTime.Now });
                isLiked = true;
            }
            else
            {
                _context.AnswerLikes.Remove(existingLike);
                isLiked = false;
            }

            await _context.SaveChangesAsync();
            var count = await _context.AnswerLikes.CountAsync(x => x.AnswerId == answerId);
            return Json(new { success = true, isLiked = isLiked, count = count });
        }

        // --- CEVAP SİSTEMİ ---

        [HttpPost]
        public IActionResult CreateAnswerAjax(string Content, int QuestionId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false, message = "Lütfen önce giriş yapınız." });

            Answer answer = new Answer
            {
                Content = Content,
                QuestionId = QuestionId,
                UserId = int.Parse(userIdStr),
                CreatedDate = DateTime.Now
            };

            _answerRepo.Add(answer);
            return Json(new { success = true, UserName = User.Identity.Name, content = answer.Content, date = answer.CreatedDate.ToString("dd.MM.yyyy HH:mm") });
        }

        [Authorize(Roles = "Admin,MainAdmin")]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _questionRepo.GetById(id);
            if (question != null) _questionRepo.Delete(question);
            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Admin,MainAdmin")]
        public IActionResult DeleteAnswer(int id)
        {
            var answer = _answerRepo.GetById(id);
            if (answer != null)
            {
                int questionId = answer.QuestionId;
                _answerRepo.Delete(answer);
                return RedirectToAction("Details", new { id = questionId });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}