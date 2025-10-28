using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."); 
builder.Services.AddDbContext<AuthDb>(options => options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentityCore<AppUser>(opt => { opt.User.RequireUniqueEmail = true; })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDb>()
    .AddDefaultTokenProviders();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-change-in-prod";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
    options.AddPolicy("UserOnly", p => p.RequireRole("USER", "ADMIN"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, new List<string>() } });
});

var app = builder.Build();

// Migrate and seed roles
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDb>();
    db.Database.Migrate();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    foreach (var role in new[] { "ADMIN", "USER" })
        if (!await roleMgr.RoleExistsAsync(role)) await roleMgr.CreateAsync(new IdentityRole(role));

    // Seed admin user
    var adminEmail = "admin@excuses.local";
    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new AppUser { UserName = "admin", Email = adminEmail };
        await userMgr.CreateAsync(admin, "Admin#12345");
        await userMgr.AddToRoleAsync(admin, "ADMIN");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/register", async (RegisterRequest req, UserManager<AppUser> users) =>
{
    var user = new AppUser { UserName = req.Username, Email = req.Email };
    var res = await users.CreateAsync(user, req.Password);
    if (!res.Succeeded) 
        return Results.BadRequest(res.Errors);
    await users.AddToRoleAsync(user, "USER");
    
    return Results.Ok(new { message = "registered" });
});

app.MapPost("/api/auth/login", async (LoginRequest req, UserManager<AppUser> users) =>
{
    var user = await users.FindByNameAsync(req.Username) ?? await users.FindByEmailAsync(req.Username);
    if (user is null || !(await users.CheckPasswordAsync(user, req.Password)))
        return Results.Unauthorized();

    var roles = await users.GetRolesAsync(user);
    var token = JwtTokenHelper.GenerateToken(user, roles, jwtKey);
    return Results.Ok(new { token });
});

app.MapGet("/api/auth/me", async (UserManager<AppUser> users, ClaimsPrincipal principal) =>
{
    var user = await users.GetUserAsync(principal);
    if (user is null) return Results.Unauthorized();
    return Results.Ok(new { user.UserName, user.Email });
}).RequireAuthorization("UserOnly");

app.Run();

// ----- types -----
record RegisterRequest(string Username, string Email, string Password);
record LoginRequest(string Username, string Password);

class AppUser : IdentityUser { }

class AuthDb : IdentityDbContext<AppUser>
{
    public AuthDb(DbContextOptions<AuthDb> options) : base(options) { }
}

static class JwtTokenHelper
{
    public static string GenerateToken(AppUser user, IEnumerable<string> roles, string key)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? ""),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}