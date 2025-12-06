using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")]
    public class AnswerController : Controller
    {
        private readonly IRepository<Answer> _repo;

        public AnswerController(IRepository<Answer> repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult Index(string searchQuestion, string searchUser)
        {
            // Önce tüm cevapları ilişkileriyle (Soru ve Üye) beraber çekiyoruz
            var answers = _repo.GetAll(x => x.User, x => x.Question);

            // 1. Soru Başlığına Göre Filtrele
            if (!string.IsNullOrEmpty(searchQuestion))
            {
                searchQuestion = searchQuestion.ToLower();
                answers = answers.Where(x =>
                    x.Question != null &&
                    x.Question.Title.ToLower().Contains(searchQuestion)
                ).ToList();
            }

            // 2. Cevap Veren Üyeye Göre Filtrele
            if (!string.IsNullOrEmpty(searchUser))
            {
                searchUser = searchUser.ToLower();
                answers = answers.Where(x =>
                    x.User != null &&
                    (x.User.Username.ToLower().Contains(searchUser) ||
                     x.User.FirstName.ToLower().Contains(searchUser) ||
                     x.User.LastName.ToLower().Contains(searchUser))
                ).ToList();
            }

            return View(answers);
        }

        public IActionResult Delete(int id)
        {
            var answer = _repo.GetById(id);
            if (answer != null)
            {
                _repo.Delete(answer);
            }
            return RedirectToAction("Index");
        }
    }
}