using _20241129612SoruCevapPortalı.Hubs;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class QuestionController : Controller
    {
        private readonly IRepository<Question> _repo;
        private readonly IRepository<Category> _catRepo;
        private readonly IHubContext<PortalHub> _hubContext; // SignalR Context tanımlı

        // Constructor'da hubContext parametre olarak eklenmeli ve atanmalı
        public QuestionController(IRepository<Question> repo, IRepository<Category> catRepo, IHubContext<PortalHub> hubContext)
        {
            _repo = repo;
            _catRepo = catRepo;
            _hubContext = hubContext; // Atama işlemi yapıldı
        }

        [HttpGet]
        public IActionResult Index(string search, int? categoryId, string searchUser)
        {
            var questions = _repo.GetAll(x => x.User, x => x.Category, x => x.Answers);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                questions = questions.Where(x => x.Title.ToLower().Contains(search)).ToList();
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                questions = questions.Where(x => x.CategoryId == categoryId.Value).ToList();
            }

            if (!string.IsNullOrEmpty(searchUser))
            {
                searchUser = searchUser.ToLower();
                questions = questions.Where(x =>
                    x.User != null &&
                    (x.User.UserName.ToLower().Contains(searchUser) ||
                     x.User.FirstName.ToLower().Contains(searchUser) ||
                     x.User.LastName.ToLower().Contains(searchUser))
                ).ToList();
            }

            ViewBag.Categories = _catRepo.GetAll();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.SearchUser = searchUser;

            return View(questions);
        }

        public async Task<IActionResult> Delete(int id) // Async yapıldı
        {
            var question = _repo.GetById(id);
            if (question != null)
            {
                string deletedTitle = question.Title;
                _repo.Delete(question);

                // İsteğe bağlı: Soru silindiğinde de adminlere bildirim gidebilir
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", User.Identity.Name, deletedTitle + " (Soru Silindi)");
            }
            return RedirectToAction("Index");
        }
    }
}