using System.Reflection;

using dailycue_api;
using dailycue_api.DTO.Requests;
using dailycue_api.Entities;

using Google.Apis.Auth;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DailyCueContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DailyCue API", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddCors();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DailyCue API v1");
    c.RoutePrefix = "swagger";
});

app.MapGet(
    "/status",
    (DailyCueContext dbContext) =>
    {
        bool dbIsConnected;
        dbIsConnected = dbContext.Database.CanConnect();

        return Results.Ok(
            new
            {
                Status = "OK",
                DatabaseConnected = dbIsConnected,
                Timestamp = DateTime.UtcNow,
            }
        );
    }
);

app.MapPost(
    "/register",
    async (DailyCueContext dbContext, RegisterUserRequest registerUser) =>
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

        return Results.Created(
            $"/users/{user.Id}",
            new { Message = "User registered successfully", User = user }
        );
    }
);
app.MapPost(
    "/login",
    (DailyCueContext dbContext, LoginUserRequest loginRequest, HttpResponse response) =>
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Email == loginRequest.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
        {
            return Results.BadRequest(new { Message = "Usuario ou senha inválidos" });
        }
        string jwtToken = "fake-jwt-token-for-demo-purposes";

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            //Secure = app.Environment.IsProduction(), // true in production
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(4)
        };
        response.Cookies.Append("Auth", jwtToken, cookieOptions);

        return Results.Ok(new { Message = "Login successful", Token = jwtToken });
    }
);
app.MapPost(
    "/google-login",
    async (DailyCueContext dbContext, TokenRequest tokenRequest) =>
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(tokenRequest.Token);
        if (payload == null)
        {
            return Results.BadRequest(new { Message = "Invalid ID token" });
        }
        var useremail = payload.Email;
        var user = dbContext.Users.FirstOrDefault(u => u.Email == useremail);
        if (user == null)
        {
            return Results.BadRequest(new { Message = "User not found" });
        }
        return Results.Ok(new { Message = "Google login successful" });
    }
);

app.MapPost(
    "/google-register",
    async (DailyCueContext dbContext, TokenRequest tokenRequest) =>
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(tokenRequest.Token);
        if (payload == null)
        {
            return Results.BadRequest(new { Message = "Invalid ID token" });
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

        return Results.Created(
            $"/users/{user.Id}",
            new { Message = "User registered successfully", User = user }
        );
    }
);

app.UseCors(policy =>
{
    _ = policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

app.Run();