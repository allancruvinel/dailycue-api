using System.Reflection;
using System.Text;

using dailycue_api;
using dailycue_api.Exceptions;
using dailycue_api.Exceptions.Handlers;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services.Configure<Env>(builder.Configuration);
var env = builder.Configuration.Get<Env>();
if (env == null)
{
    throw new Exception("Fail to load environment variables");
}
env.Validate();
builder.Services.AddSingleton(env);

builder.Services.AddDbContext<DailyCueContext>();

builder.Services.AddEndpointsApiExplorer();

// Configure JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = env.Jwt.Issuer,
            ValidAudience = env.Jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(env.Jwt.Key)) // Your secret key
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {

                if (context.Request.Cookies.ContainsKey("Auth"))
                {
                    context.Token = context.Request.Cookies["Auth"];
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

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
        var assembly = Assembly.GetEntryAssembly();
        bool dbIsConnected;
        dbIsConnected = dbContext.Database.CanConnect();

        return Results.Ok(
            new
            {
                Status = "OK",
                DatabaseConnected = dbIsConnected,
                Timestamp = DateTime.UtcNow,
                Version = assembly?.GetName().Version?.ToString() ?? "unknown"
            }
        );
    }
);
app.MapGet("/me", [Authorize] (DailyCueContext dbContext, HttpContext httpContext) =>
{
    var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "id");
    if (userIdClaim == null)
    {
        return Results.Unauthorized();
    }
    var userId = Guid.Parse(userIdClaim.Value);
    var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
    if (user == null)
    {
        return Results.NotFound(new { Message = "User not found" });
    }
    return Results.Ok(new { id = user.Id, name = user.Name, email = user.Email });
});


app.MapControllers();
app.Run();