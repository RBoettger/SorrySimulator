using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmailService", Version = "v1" }));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/email/send", (EmailRequest req) =>
{
    // Stub de envio: apenas loga; configure SMTP real depois.
    Console.WriteLine($"[EmailService] To: {req.To}, Subject: {req.Subject}");
    return Results.Ok(new { sent = true });
});

app.Run();

record EmailRequest(string To, string Subject, string Body);