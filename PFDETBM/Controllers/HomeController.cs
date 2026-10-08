using Microsoft.AspNetCore.Mvc;

namespace PFDETBM.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
