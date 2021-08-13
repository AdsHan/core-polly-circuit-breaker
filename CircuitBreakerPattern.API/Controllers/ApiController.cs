using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace CircuitBreakerPattern.API.Controllers
{
    [ApiController]
    [Route("api/polly-test")]
    public class ResourceController : ControllerBase
    {
        private readonly ILogger<ResourceController> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public ResourceController(IHttpClientFactory clientFactory, ILogger<ResourceController> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var result = await _clientFactory.CreateClient("PollyTest").GetAsync("/api/resource");

            if (result.IsSuccessStatusCode)
            {
                return Ok();
            }

            return NoContent();
        }
    }
}


