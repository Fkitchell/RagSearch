using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace RagSearch.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var versionInfo = typeof(HealthController)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

            return Ok(new
            {
                Status = "Healthy",
                BuildVersion = versionInfo
            });
        }
    }
}
