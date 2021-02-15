using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class PersonController : Controller
    {
        [HttpPost("[action]")]
        public IActionResult Register(Person user)
        {
            return Ok();
        }
    }
}
