
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileChunksController : ControllerBase
{
    private readonly IFileChunkService _fileChunkService;

    public FileChunksController(IFileChunkService fileChunkService)
    {
        _fileChunkService = fileChunkService;
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return int.TryParse(userIdClaim, out userId);
    }

    [HttpGet("file/{storedFileId:guid}")]
    public async Task<IActionResult> GetChunks(Guid storedFileId)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var chunks = await _fileChunkService.GetChunksAsync(
            userId,
            storedFileId);

        return Ok(chunks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetChunk(Guid id)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var chunk = await _fileChunkService.GetChunkByIdAsync(
            userId,
            id);

        if (chunk == null)
            return NotFound(new
            {
                message = "File chunk not found."
            });

        return Ok(chunk);
    }

    [HttpPost]
    public async Task<IActionResult> CreateChunk(
        [FromBody] CreateFileChunkDto dto)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        try
        {
            var chunk = await _fileChunkService.CreateChunkAsync(
                userId,
                dto);

            if (chunk == null)
                return NotFound(new
                {
                    message = "Stored file not found."
                });

            return CreatedAtAction(
                nameof(GetChunk),
                new { id = chunk.Id },
                chunk);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}
