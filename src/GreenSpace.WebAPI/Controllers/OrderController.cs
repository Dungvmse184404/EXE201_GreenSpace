using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
