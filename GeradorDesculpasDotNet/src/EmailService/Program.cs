using Microsoft.OpenApi.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using EmailService.Data;
using EmailService.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("Default")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EmailDb>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmailService", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();

app.MapPost("/api/email/send", async (EmailRequest req, IConfiguration config, EmailDb db, HttpContext httpContext) =>
{
    var apiKey = config["SENDGRID_API_KEY"];

    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.BadRequest("SENDGRID_API_KEY não configurada.");

    var fromEmail = config["SMTP_FROM"];
    var fromName  = config["SMTP_FROM_NAME"];

    if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(fromName))
        return Results.BadRequest("Variáveis SMTP_FROM e SMTP_FROM_NAME não configuradas.");

    var client = new SendGridClient(apiKey);

    var from = new EmailAddress(fromEmail, fromName);
    var to   = new EmailAddress(req.To);

    var msg = MailHelper.CreateSingleEmail(from, to, req.Subject, req.Body, req.Body);
    var response = await client.SendEmailAsync(msg);

    var body = await response.Body.ReadAsStringAsync();

    int? userId = null;
    if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var headerValues) && int.TryParse(headerValues.ToString(), out var parsedId))
        userId = parsedId;

    var statusCode = (int)response.StatusCode;
    if (statusCode >= 200 && statusCode < 300)
    {
        try
        {
            var brasilia = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            var agoraBrasil = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brasilia);

            var history = new GdExcuseHistory
            {
                 UserId     = userId,
                SenderName = fromName,       
                ToEmail    = req.To,
                Subject    = req.Subject,
                Motive     = null,          
                Tone       = null,           
                ExcuseText = req.Body,
                SentAt     = agoraBrasil
            };

            db.ExcuseHistory.Add(history);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao salvar histórico de e-mail: {ex.Message}");
        }
    }

    return Results.Ok(new
    {
        Status   = response.StatusCode,
        Response = body
    });
});

app.MapGet("/api/email/history", async (HttpRequest req, EmailDb db) =>
{
    var userIdHeader = req.Headers["X-User-Id"].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(userIdHeader))
        return Results.BadRequest("UserId não enviado.");

    if (!int.TryParse(userIdHeader, out var userId))
        return Results.BadRequest("UserId inválido.");

    var history = await db.ExcuseHistory
        .Where(x => x.UserId == userId)
        .OrderByDescending(x => x.SentAt)
        .ToListAsync();

    return Results.Ok(history);
});

app.Run();

public record EmailRequest(string To, string Subject, string Body);
