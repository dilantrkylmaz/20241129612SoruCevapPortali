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
            
                var userId = User.FindFirstValue("UserId");
                p.UserId = int.Parse(userId);
                p.CreatedDate = DateTime.Now;

                _questionRepo.Add(p);
                return RedirectToAction("Index", "Home");
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
            return RedirectToAction("Details", new { id = QuestionId });
        }

        [HttpPost]
        public IActionResult CreateAnswerAjax(string Content, int QuestionId)
        {
            var userId = User.FindFirstValue("UserId");

            if (userId == null)
            {
                return Json(new { success = false, message = "Lütfen önce giriş yapınız." });
            }

            Answer answer = new Answer
            {
                Content = Content,
                QuestionId = QuestionId,
                UserId = int.Parse(userId),
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