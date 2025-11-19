using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AuthService.Data;
using AuthService.Helpers;
using AuthService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AuthDb>(opt => opt.UseSqlServer(connectionString));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

var corsPolicyName = "AllowFrontend";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors(corsPolicyName);
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/login", async (AuthDb db, IConfiguration config, LoginRequest req) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.IsActive);
    if (user is null) return Results.Unauthorized();

    var valid = PasswordHasher.VerifyPassword(req.Password, user.PasswordHash, user.PasswordSalt, user.HashIterations);
    if (!valid) return Results.Unauthorized();

    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "dev-secret-key");
    var descriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim("UserId", user.UserId.ToString()),
            new System.Security.Claims.Claim("Email", user.Email),
            new System.Security.Claims.Claim("Name", user.Name)
        }),
        Expires = DateTime.UtcNow.AddHours(12),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(descriptor);
    return Results.Ok(new { token = tokenHandler.WriteToken(token) });
})
.WithName("Login");

app.MapPost("/api/auth/register", async (AuthDb db, RegisterRequest request) =>
{
    if (await db.Users.AnyAsync(u => u.Email == request.Email))
        return Results.BadRequest("E-mail já cadastrado.");

    var (hash, salt, algo, iterations) = PasswordHasher.HashPassword(request.Password);

    var user = new GdUser
    {
        Name = request.Name,
        Email = request.Email,
        PasswordHash = hash,
        PasswordSalt = salt,
        PasswordAlgo = algo,
        HashIterations = iterations,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("Usuário registrado com sucesso.");
})
.WithName("Register");

app.Run();

record LoginRequest(string Email, string Password);