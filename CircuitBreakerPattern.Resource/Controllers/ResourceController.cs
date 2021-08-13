using Microsoft.AspNetCore.Mvc;

namespace CircuitBreakerPattern.Resource.Controllers
{
    [ApiController]
    [Route("api/resource")]
    public class ResourceController : ControllerBase
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ActionResult Get()
        {
            return NotFound();
        }
    }
}
