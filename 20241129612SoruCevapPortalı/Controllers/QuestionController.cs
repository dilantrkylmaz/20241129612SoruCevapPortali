using _20241129612SoruCevapPortalı.Hubs;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public QuestionController(IRepository<Question> questionRepo, IRepository<Category> categoryRepo, IRepository<Answer> answerRepo, IHubContext<PortalHub> hubContext, AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _questionRepo = questionRepo;
            _categoryRepo = categoryRepo;
            _answerRepo = answerRepo;
            _hubContext = hubContext;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Create(Question p, IFormFile? mediaFile)
        {
            ModelState.Remove("UserId");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("Answers");
            ModelState.Remove("QuestionLikes");

            if (ModelState.IsValid)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdStr))
                {
                    p.UserId = int.Parse(userIdStr);
                    p.CreatedDate = DateTime.Now;

                    if (mediaFile != null)
                    {
                        p.ImageUrl = await SaveFile(mediaFile, "questions");
                    }

                    _questionRepo.Add(p);

                    var user = _context.Users.Find(p.UserId);
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", user?.UserName ?? "Anonim", p.Title);

                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name", p.CategoryId);
            return View(p);
        }


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
        public async Task<IActionResult> Edit(Question model, IFormFile? newMedia, bool removeCurrentMedia)
        {
            var question = _questionRepo.GetById(model.Id);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (question == null || (question.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("MainAdmin"))) return Forbid();

            ModelState.Remove("User");
            ModelState.Remove("Category");
            ModelState.Remove("Answers");
            ModelState.Remove("QuestionLikes");

            if (ModelState.IsValid)
            {
                question.Title = model.Title;
                question.Content = model.Content;
                question.CategoryId = model.CategoryId;

                if (removeCurrentMedia) { question.ImageUrl = null; }
                else if (newMedia != null) { question.ImageUrl = await SaveFile(newMedia, "questions"); }

                _questionRepo.Update(question);
                return RedirectToAction("Details", new { id = question.Id });
            }
            ViewBag.Categories = new SelectList(_categoryRepo.GetAll(), "Id", "Name", model.CategoryId);
            return View(model);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAnswerAjax(string Content, int QuestionId, IFormFile? answerImage)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Json(new { success = false });

            var answer = new Answer
            {
                Content = Content,
                QuestionId = QuestionId,
                UserId = int.Parse(userIdStr),
                CreatedDate = DateTime.Now
            };

            if (answerImage != null) { answer.ImageUrl = await SaveFile(answerImage, "answers"); }

            _answerRepo.Add(answer);
            return Json(new { success = true });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditAnswerAjax(int id, string content, IFormFile? newImage, bool removeImage)
        {
            var answer = _answerRepo.GetById(id);
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (answer == null || (answer.UserId != userId && !User.IsInRole("Admin"))) return Json(new { success = false });

            answer.Content = content;
            if (removeImage) { answer.ImageUrl = null; }
            else if (newImage != null) { answer.ImageUrl = await SaveFile(newImage, "answers"); }

            _answerRepo.Update(answer);
            return Json(new { success = true });
        }


        public IActionResult Details(int id)
        {
            var question = _questionRepo.Get(x => x.Id == id, "User", "Category", "Answers", "Answers.User", "QuestionLikes", "Answers.AnswerLikes");
            if (question == null) return NotFound();

            DateTime filterDate = DateTime.Now.AddMonths(-1);
            ViewBag.PopularQuestions = _questionRepo.GetAll(x => x.QuestionLikes).Where(x => x.CreatedDate >= filterDate).OrderByDescending(x => x.QuestionLikes.Count).Take(5).ToList();
            return View(question);
        }


        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            string extension = Path.GetExtension(file.FileName);
            string fileName = Guid.NewGuid().ToString() + extension;
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "img", folder);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            using (var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create)) { await file.CopyToAsync(stream); }
            return $"/img/{folder}/{fileName}";
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> LikeQuestionAjax(int questionId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingLike = await _context.QuestionLikes.FirstOrDefaultAsync(x => x.QuestionId == questionId && x.UserId == userId);
            if (existingLike == null) _context.QuestionLikes.Add(new QuestionLike { QuestionId = questionId, UserId = userId, CreatedDate = DateTime.Now });
            else _context.QuestionLikes.Remove(existingLike);
            await _context.SaveChangesAsync();
            return Json(new { success = true, count = _context.QuestionLikes.Count(x => x.QuestionId == questionId) });
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
            return Json(new { success = true, count = _context.AnswerLikes.Count(x => x.AnswerId == answerId) });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ReportAjax(int? questionId, int? answerId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return Json(new { success = false, message = "Neden belirtiniz." });
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _context.Reports.Add(new Report { ReporterUserId = userId, QuestionId = questionId, AnswerId = answerId, Reason = reason, CreatedDate = DateTime.Now });
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Şikayetiniz iletildi." });
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
            if (answer != null) { int qId = answer.QuestionId; _answerRepo.Delete(answer); return RedirectToAction("Details", new { id = qId }); }
            return RedirectToAction("Index", "Home");
        }
    }
}