using Microsoft.AspNetCore.Mvc;

namespace ARBusinessCard.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            return View();
        }

        public IActionResult PageNotFound()
        {
            return View();
        }
    }
}
