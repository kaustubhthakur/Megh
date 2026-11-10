
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/google-drive")]
[Authorize]
public class GoogleDriveController : ControllerBase
{
    [HttpGet("connect")]
    public IActionResult Connect()
    {
        return Ok("Google Drive connect route is working.");
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public IActionResult Callback(
        [FromQuery] string? code,
        [FromQuery] string? state)
    {
        return Ok(new
        {
            message = "Google Drive callback route is working.",
            codeReceived = !string.IsNullOrEmpty(code),
            stateReceived = !string.IsNullOrEmpty(state)
        });
    }
}
