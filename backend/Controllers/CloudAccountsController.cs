
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CloudAccountsController : ControllerBase
{
    private readonly IConnectedAccountService _accountService;

    public CloudAccountsController(
        IConnectedAccountService accountService)
    {
        _accountService = accountService;
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
    public async Task<IActionResult> GetAccounts()
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var accounts =
            await _accountService.GetAccountsAsync(userId);

        return Ok(accounts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetAccount(int id)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var account =
            await _accountService.GetAccountByIdAsync(userId, id);

        if (account == null)
            return NotFound(new
            {
                message = "Connected account not found."
            });

        return Ok(account);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DisconnectAccount(int id)
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var disconnected =
            await _accountService.DisconnectAccountAsync(userId, id);

        if (!disconnected)
            return NotFound(new
            {
                message = "Connected account not found."
            });

        return Ok(new
        {
            message = "Cloud account disconnected successfully."
        });
    }


    [HttpGet("storage-summary")]
    public async Task<IActionResult> GetStorageSummary()
    {
        if (!TryGetUserId(out int userId))
            return Unauthorized();

        var summary =
            await _accountService.GetStorageSummaryAsync(userId);

        return Ok(summary);
    }
}
