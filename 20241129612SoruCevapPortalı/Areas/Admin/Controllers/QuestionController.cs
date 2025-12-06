using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using _20241129612SoruCevapPortalı.Models;
using _20241129612SoruCevapPortalı.Repositories.Abstract;

namespace _20241129612SoruCevapPortalı.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,MainAdmin")] // Sadece yöneticiler girebilir
    public class QuestionController : Controller
    {
        private readonly IRepository<Question> _questionRepo;

        public QuestionController(IRepository<Question> questionRepo)
        {
            _questionRepo = questionRepo;
        }

        // SORULARI LİSTELE
        public IActionResult Index(string search)
        {
            // Önce ilişkili verilerle (User, Category) hepsini çekiyoruz
            var questions = _questionRepo.GetAll(x => x.User, x => x.Category);

            // Eğer arama yapılmışsa listeyi filtreliyoruz
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                questions = questions.Where(x => x.Title.ToLower().Contains(search)).ToList();
            }

            return View(questions);
        }

        // SORUYU SİL
        public IActionResult Delete(int id)
        {
            var question = _questionRepo.GetById(id);
            if (question != null)
            {
                _questionRepo.Delete(question);
            }
            return RedirectToAction("Index");
        }
    }
}