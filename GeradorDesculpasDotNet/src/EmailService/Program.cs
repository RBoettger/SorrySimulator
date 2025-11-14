using Microsoft.OpenApi.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

var builder = WebApplication.CreateBuilder(args);
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

app.MapPost("/api/email/send", async (EmailRequest req, IConfiguration config) =>
{
    var apiKey = config["SENDGRID_API_KEY"];

    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.BadRequest("SENDGRID_API_KEY não configurada.");

    var fromEmail = config["SMTP_FROM"];
    var fromName = config["SMTP_FROM_NAME"];

    if (string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(fromName))
        return Results.BadRequest("Variáveis SMTP_FROM e SMTP_FROM_NAME não configuradas.");

    var client = new SendGridClient(apiKey);

    var from = new EmailAddress(fromEmail, fromName);
    var to = new EmailAddress(req.To);

    var msg = MailHelper.CreateSingleEmail(from, to, req.Subject, req.Body, req.Body);
    var response = await client.SendEmailAsync(msg);

    var body = await response.Body.ReadAsStringAsync();

    return Results.Ok(new
    {
        Status = response.StatusCode,
        Response = body
    });
});

app.Run();

record EmailRequest(string To, string Subject, string Body);