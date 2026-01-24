using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
