using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestWebApiApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class SecureController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            return Ok();
        }
    }
}
