using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
