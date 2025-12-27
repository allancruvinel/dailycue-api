using dailycue_api.DTO.Requests;
using dailycue_api.Entities;
using dailycue_api.Utils;

using Google.Apis.Auth;

using Microsoft.AspNetCore.Mvc;

namespace dailycue_api.Controllers;

[ApiController]
public class AuthController(DailyCueContext dbContext, Env env) : ControllerBase
{
    [HttpPost("/register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserRequest registerUser)
    {
        var user = new User
        {
            Name = registerUser.Name,
            Email = registerUser.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerUser.Password, workFactor: 12),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Created(
            $"/users/{user.Id}",
            new { Message = "User registered successfully", User = user }
        );
    }

    [HttpPost("/login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginUserRequest loginRequest)
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Email == loginRequest.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
        {
            return BadRequest(new { Message = "Usuario ou senha inválidos" });
        }


        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            //Secure = app.Environment.IsProduction(), // true in production
            Secure = false,
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(4)
        };

        var jwtToken = JwtTokenGenerator.GenerateToken(
            user,
            env.Jwt.Key,
            env.Jwt.Issuer,
            env.Jwt.Audience
        );
        Response.Cookies.Append("Auth", jwtToken, cookieOptions);

        return Ok(new { Message = "Login successful", Token = jwtToken });
    }

    [HttpPost("/google-login")]
    public async Task<IActionResult> GoogleLoginAsync([FromBody] TokenRequest tokenRequest)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(tokenRequest.Token);
        if (payload == null)
        {
            return BadRequest(new { Message = "Invalid ID token" });
        }
        var useremail = payload.Email;
        var user = dbContext.Users.FirstOrDefault(u => u.Email == useremail);
        if (user == null)
        {
            return BadRequest(new { Message = "User not found" });
        }
        var jwtToken = JwtTokenGenerator.GenerateToken(
            user,
            env.Jwt.Key,
            env.Jwt.Issuer,
            env.Jwt.Audience
        );

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            //Secure = app.Environment.IsProduction(), // true in production
            Secure = false,
            Path = "/",
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddHours(4)
        };

        Response.Cookies.Append("Auth", jwtToken, cookieOptions);
        return Ok(new { Message = "Google login successful", Token = jwtToken });
    }

    [HttpPost("/google-register")]
    public async Task<IActionResult> GoogleRegisterAsync([FromBody] TokenRequest tokenRequest)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(tokenRequest.Token);
        if (payload == null)
        {
            return BadRequest(new { Message = "Invalid ID token" });
        }
        var user = new User
        {
            Name = payload.Name,
            Email = payload.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                Guid.NewGuid().ToString(),
                workFactor: 12
            ),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Created(
            $"/users/{user.Id}",
            new { Message = "User registered successfully", User = user }
        );
    }

}