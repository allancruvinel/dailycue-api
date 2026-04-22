using dailycue_api.DTO.Requests;
using dailycue_api.Entities;
using dailycue_api.Utils;

using Google.Apis.Auth;

using Microsoft.AspNetCore.Mvc;

namespace dailycue_api.Controllers.V1;

[ApiController]
[Route("v1")]
public class AuthController(DailyCueContext dbContext, Env env) : ControllerBase
{
    [HttpPost("register")]
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
            $"/v1/users/{user.Id}",
            new
            {
                Message = "Usuário registrado com sucesso",
                User = ToUserResponse(user)
            }
        );
    }

    [HttpPost("login")]
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

        return Ok(new
        {
            Message = "Login realizado com sucesso",
            Token = jwtToken,
            User = ToUserResponse(user)
        });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLoginAsync([FromBody] TokenRequest tokenRequest)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(tokenRequest.Token);
        if (payload == null)
        {
            return BadRequest(new { Message = "Token de autenticação inválido" });
        }
        var useremail = payload.Email;
        var user = dbContext.Users.FirstOrDefault(u => u.Email == useremail);
        if (user == null)
        {
            return BadRequest(new { Message = "Usuário não encontrado" });
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
        return Ok(new
        {
            Message = "Login com Google realizado com sucesso",
            Token = jwtToken,
            User = ToUserResponse(user)
        });
    }

    [HttpPost("google-register")]
    public async Task<IActionResult> GoogleRegisterAsync([FromBody] TokenRequest tokenRequest)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(tokenRequest.Token);
        if (payload == null)
        {
            return BadRequest(new { Message = "Token de autenticação inválido" });
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
            $"/v1/users/{user.Id}",
            new
            {
                Message = "Usuário registrado com Google com sucesso",
                User = ToUserResponse(user)
            }
        );
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("Auth");
        return Ok(new { Message = "Logout realizado com sucesso" });
    }

    private static object ToUserResponse(User user)
    {
        return new
        {
            user.Id,
            user.Name,
            user.Email,
            user.CreatedAt,
            user.UpdatedAt
        };
    }
}