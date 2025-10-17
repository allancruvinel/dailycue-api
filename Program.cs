using System.Reflection;
using dailycue_api;
using dailycue_api.Entities;
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
    async (DailyCueContext dbContext, User user) =>
    {
        //logic register here

        return Results.Ok(new { Message = "User registered successfully", User = user });
    }
);
app.MapPost(
    "/login",
    async (DailyCueContext dbContext, string username, string password) =>
    {
        //logic login here
        return Results.Ok();
    }
);
app.MapPost(
    "/google-login",
    (string idToken) =>
    {
        // Logic to validate the ID token with Google's OAuth2 API
        return Results.Ok(new { Message = "Google login successful", IdToken = idToken });
    }
);

app.Run();
