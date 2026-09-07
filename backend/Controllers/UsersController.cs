using EcSite.Api.Data;
using EcSite.Api.DTOs.Users;
using EcSite.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcSite.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    [HttpGet("me")]
    public async Task<ActionResult<MeDto>> Me()
    {
        var userId = User.GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        return Ok(new MeDto(user.Id, user.Email, user.Name, user.Role.ToString(), user.Points));
    }

    [HttpPut("me")]
    public async Task<ActionResult<MeDto>> UpdateProfile(UpdateProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "ニックネームを入力してください" });

        var userId = User.GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        user.Name = request.Name.Trim();
        await _db.SaveChangesAsync();

        return Ok(new MeDto(user.Id, user.Email, user.Name, user.Role.ToString(), user.Points));
    }

    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(new { message = "新しいパスワードは6文字以上にしてください" });

        var userId = User.GetUserId();
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "現在のパスワードが正しくありません" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
