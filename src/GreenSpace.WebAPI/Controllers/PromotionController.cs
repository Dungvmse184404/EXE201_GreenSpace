using Microsoft.AspNetCore.Mvc;

namespace GreenSpace.WebAPI.Controllers
{
    public class PromotionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
