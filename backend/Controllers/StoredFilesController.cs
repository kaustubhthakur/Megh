
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoredFilesController : ControllerBase
{
    private readonly IStoredFileService _storedFileService;

    public StoredFilesController(IStoredFileService storedFileService)
    {
        _storedFileService = storedFileService;
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return int.TryParse(userIdClaim, out userId);
    }

    [HttpGet]
    public async Task<IActionResult> GetFiles()
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var files = await _storedFileService.GetFilesAsync(userId);

        return Ok(files);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var file = await _storedFileService.GetFileByIdAsync(
            userId,
            id);

        if (file == null)
        {
            return NotFound(new
            {
                message = "Stored file not found."
            });
        }

        return Ok(file);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFile(
        [FromBody] CreateStoredFileDto dto)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var file = await _storedFileService.CreateFileAsync(
            userId,
            dto);

        return CreatedAtAction(
            nameof(GetFile),
            new { id = file.Id },
            file);
    }
}
