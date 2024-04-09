using Microsoft.AspNetCore.Mvc;

namespace RacerBooksAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
