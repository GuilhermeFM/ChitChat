using System.Security.Claims;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class MessageController : Controller
    {
        [HttpGet("[action]")]
        public IActionResult Message(Person user)
        {
            return Ok();
        }

        [HttpPost("[action]")]
        public IActionResult Message(Message message)
        {
            var claim = User.FindFirst(ClaimTypes.UserData);
            return Ok(claim.Value);
        }
    }
}
