using Core.Models;

using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("[controller]")]
    public class UserController : Controller
    {
        [HttpPost("[action]")]
        public IActionResult Register(User user)
        {
            return Ok();
        }
    }
}
