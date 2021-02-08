using Core.Models;

using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("[controller]")]
    public class MessageController : Controller
    {
        [HttpGet("[action]")]
        public IActionResult Message(User user)
        {
            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult Message(Message message)
        {
            return Ok();
        }
    }
}
