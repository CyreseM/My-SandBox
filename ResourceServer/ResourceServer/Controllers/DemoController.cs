using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ResourceServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        // GET: /api/demo/public-data
        // This endpoint is open and does not require any authentication.
        [HttpGet("public-data")]
        public IActionResult GetPublicData()
        {
            var publicData = new
            {
                Message = "This is public data accessible without authentication."
            };
            return Ok(publicData);
        }

        // GET: /api/demo/protected-data
        // This endpoint is secured and requires a valid JWT token.
        [Authorize]
        [HttpGet("protected-data")]
        public IActionResult GetProtectedData()
        {
            var protectedData = new
            {
                Message = "This is protected data accessible only with a valid JWT token."
            };
            return Ok(protectedData);
        }
    }
}