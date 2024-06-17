using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace SprayingSystem.Controllers
{
    public class ShutdownController : Controller
    {
        private readonly IHostApplicationLifetime _appLifetime;

        public ShutdownController(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
        }

        [HttpPost] // Ensure this endpoint is secure and not publicly accessible
        public IActionResult Shutdown()
        {
            _appLifetime.StopApplication();
            return Ok("Application is shutting down.");
        }
    }
}
