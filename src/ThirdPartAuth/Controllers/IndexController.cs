using Microsoft.AspNetCore.Mvc;

namespace ThirdPartAuth.Controllers
{
    [ApiController]
    [Route("")]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Welcome!");
        }
    }
}
