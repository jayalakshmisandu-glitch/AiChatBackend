using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using AiChatBackend.DAL;
using AiChatBackend.DTOs;
using AiChatBackend.Models;
using MongoDB.Driver;


namespace AiChatBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AuthController(MongoDbContext context)
        {
            _context = context;
        }

      
        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Users
                .Find(x => x.Email == dto.Email)
                .FirstOrDefaultAsync();

            if (existing != null)
                return BadRequest("User already exists");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.InsertOneAsync(user);

            return Ok("User created successfully");
        }

        // ✅ LOGIN (creates cookie)
  
       

[HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users
            .Find(x => x.Email == dto.Email)
            .FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized();

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email)
    };

        var identity = new ClaimsIdentity(claims, "cookie");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("cookie", principal); // 🔥 THIS IS KEY

        return Ok("Login success");
    }

    // ✅ LOGOUT
    [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("cookie");
            return Ok();
        }
    }
}
