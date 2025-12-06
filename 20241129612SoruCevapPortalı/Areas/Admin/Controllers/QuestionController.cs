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
        public IActionResult Index()
        {
            // Kullanıcı, Kategori ve Cevap sayılarını getir
            var questions = _questionRepo.GetAll(x => x.User, y => y.Category, z => z.Answers);
            return View(questions.OrderByDescending(x => x.CreatedDate).ToList());
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