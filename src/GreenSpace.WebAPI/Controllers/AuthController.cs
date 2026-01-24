using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
