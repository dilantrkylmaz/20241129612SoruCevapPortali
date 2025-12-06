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

        public IActionResult Index()
        {
            // Cevapları getirirken kimin yazdığını (User) ve hangi soruya yazdığını (Question) da getiriyoruz.
            var answers = _repo.GetAll(x => x.User, x => x.Question);
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