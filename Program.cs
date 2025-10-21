using System.Reflection;
using dailycue_api;
using dailycue_api.DTO.Reponses;
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
    (DailyCueContext dbContext, string email, string password) =>
    {
        var user = dbContext.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return Results.BadRequest(new { Message = "Invalid email or password" });
        }
        return Results.Ok(new { Message = "Login successful" });
    }
);
app.MapPost(
    "/google-login",
    async (DailyCueContext dbContext, string idToken) =>
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
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
    (string idToken) =>
    {
        // Logic to validate the ID token with Google's OAuth2 API and register the user
        return Results.Ok(new { Message = "Google registration successful", IdToken = idToken });
    }
);

app.Run();
