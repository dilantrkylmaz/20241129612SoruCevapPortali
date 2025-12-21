using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.SignalR; // SignalR için eklendi
using _20241129612SoruCevapPortalı.Hubs; // Hub erişimi için eklendi

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Answer> _answerRepo;
        private readonly IHubContext<PortalHub> _hubContext; // SignalR eklendi

        // Constructor güncellendi
        public QuestionController(IRepository<Question> questionRepo, IRepository<Category> categoryRepo, IRepository<Answer> answerRepo, IHubContext<PortalHub> hubContext)
        {
            _questionRepo = questionRepo;
            _categoryRepo = categoryRepo;
            _answerRepo = answerRepo;
            _hubContext = hubContext;
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
        public async Task<IActionResult> Create(Question p) // SignalR için async yapıldı
        {
            if (ModelState.IsValid)
            {
                // HATA BURADAYDI: "UserId" yerine ClaimTypes.NameIdentifier kullanılmalı
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userIdStr))
                {
                    p.UserId = int.Parse(userIdStr);
                    p.CreatedDate = DateTime.Now;

                    _questionRepo.Add(p);

                    // SIGNALR BİLDİRİMİ (Final İş Paketi)
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", User.Identity.Name, p.Title);

                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name");
            return View(p);
        }

        public IActionResult Details(int id)
        {
            var question = _questionRepo.Get(x => x.Id == id, "User", "Category", "Answers", "Answers.User");
            if (question == null) return NotFound();
            return View(question);
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateAnswer(string Content, int QuestionId)
        {
            if (!string.IsNullOrEmpty(Content))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); // Düzenlendi

                Answer answer = new Answer
                {
                    Content = Content,
                    QuestionId = QuestionId,
                    UserId = int.Parse(userIdStr),
                    CreatedDate = DateTime.Now
                };

                _answerRepo.Add(answer);
            }
            return RedirectToAction("Details", new { id = QuestionId });
        }

        [HttpPost]
        public IActionResult CreateAnswerAjax(string Content, int QuestionId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier); // Düzenlendi

            if (string.IsNullOrEmpty(userIdStr))
            {
                return Json(new { success = false, message = "Lütfen önce giriş yapınız." });
            }

            Answer answer = new Answer
            {
                Content = Content,
                QuestionId = QuestionId,
                UserId = int.Parse(userIdStr),
                CreatedDate = DateTime.Now
            };

            _answerRepo.Add(answer);

            return Json(new
            {
                success = true,
                UserName = User.Identity.Name,
                content = answer.Content,
                date = answer.CreatedDate.ToString("dd.MM.yyyy HH:mm")
            });
        }

        [Authorize(Roles = "Admin,MainAdmin")]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _questionRepo.GetById(id);
            if (question != null)
            {
                _questionRepo.Delete(question);
            }
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