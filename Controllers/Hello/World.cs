using Microsoft.AspNetCore.Mvc;

namespace VelocityAPI.Controllers.Hello;

[ApiController]
[Route("/api/hello/[controller]")]
public class WorldController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHello([FromQuery] string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return Ok($"Hello, {name}!");
        }
        return Ok("Hello, World!");
    }
}
