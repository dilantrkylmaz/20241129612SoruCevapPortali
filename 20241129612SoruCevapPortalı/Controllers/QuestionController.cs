using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Controllers
{
    public class QuestionController : Controller
    {
        private readonly IRepository<Question> _questionRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Answer> _answerRepo;

        public QuestionController(IRepository<Question> questionRepo, IRepository<Category> categoryRepo, IRepository<Answer> answerRepo)
        {
            _questionRepo = questionRepo;
            _categoryRepo = categoryRepo;
            _answerRepo = answerRepo;
        }

        // 1. SORU SORMA SAYFASI (Giriş yapmışlar görebilir)
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name");
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Create(Question p)
        {
            if (ModelState.IsValid)
            {
                // Giriş yapan kullanıcının ID'sini al
                var userId = User.FindFirstValue("UserId");
                p.UserId = int.Parse(userId);
                p.CreatedDate = DateTime.Now;

                _questionRepo.Add(p);
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name");
            return View(p);
        }

        // 2. SORU DETAYI VE CEVAPLARI GÖRME
        public IActionResult Details(int id)
        {
            // Soruyu, soran kullanıcıyı ve cevapları (ve cevabı yazanları) getir
            var question = _questionRepo.Get(x => x.Id == id, "User", "Category", "Answers", "Answers.User");

            if (question == null) return NotFound();

            return View(question);
        }

        // 3. CEVAP YAZMA İŞLEMİ
        [Authorize]
        [HttpPost]
        public IActionResult CreateAnswer(string Content, int QuestionId)
        {
            if (!string.IsNullOrEmpty(Content))
            {
                var userId = User.FindFirstValue("UserId");

                Answer answer = new Answer
                {
                    Content = Content,
                    QuestionId = QuestionId,
                    UserId = int.Parse(userId),
                    CreatedDate = DateTime.Now
                };

                _answerRepo.Add(answer);
            }
            // Sayfayı yenileyip detaya geri dön
            return RedirectToAction("Details", new { id = QuestionId });
        }

        // --- YENİ EKLENEN: SORU SİLME (Sadece Admin ve MainAdmin) ---
        [Authorize(Roles = "Admin,MainAdmin")]
        public IActionResult DeleteQuestion(int id)
        {
            var question = _questionRepo.GetById(id);
            if (question != null)
            {
                // Repository'de cascade delete ayarlıysa cevaplar da otomatik gider
                _questionRepo.Delete(question);
            }
            return RedirectToAction("Index", "Home");
        }

        // --- YENİ EKLENEN: CEVAP SİLME (Sadece Admin ve MainAdmin) ---
        [Authorize(Roles = "Admin,MainAdmin")]
        public IActionResult DeleteAnswer(int id)
        {
            var answer = _answerRepo.GetById(id);
            if (answer != null)
            {
                int questionId = answer.QuestionId; // Silindikten sonra soruya geri dönmek için ID'yi alıyoruz
                _answerRepo.Delete(answer);
                return RedirectToAction("Details", new { id = questionId });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}