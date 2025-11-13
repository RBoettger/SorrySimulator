using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmailService", Version = "v1" }));

// Opções de SMTP vindas de appsettings/env vars (prefixo "Smtp")
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Ok("EmailService OK"));

app.MapPost("/api/email/send", async (EmailRequest req, IEmailSender sender, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(req.To) || string.IsNullOrWhiteSpace(req.Subject))
        return Results.BadRequest("Campos 'to' e 'subject' são obrigatórios.");

    await sender.SendAsync(req.To, req.Subject, req.Body ?? "", ct);
    return Results.Ok(new { sent = true });
});

app.Run();

/// Models / Services

public record EmailRequest(string To, string Subject, string? Body);

public class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;          
    public bool UseStartTls { get; set; } = true; 
    public bool UseSsl { get; set; } = false;     
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";        
    public string FromName { get; set; } = "EmailService";
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}

public class MailKitEmailSender : IEmailSender
{
    private readonly SmtpOptions _opt;
    public MailKitEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.Host) || string.IsNullOrWhiteSpace(_opt.From))
            throw new InvalidOperationException("SMTP não configurado (Host/From).");

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opt.FromName ?? "", _opt.From));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;

        var body = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = StripHtml(htmlBody)
        };
        msg.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        var secure =
            _opt.UseSsl ? SecureSocketOptions.SslOnConnect :
            _opt.UseStartTls ? SecureSocketOptions.StartTls :
            SecureSocketOptions.Auto;

        await client.ConnectAsync(_opt.Host, _opt.Port, secure, ct);

        if (!string.IsNullOrEmpty(_opt.User))
            await client.AuthenticateAsync(_opt.User, _opt.Password, ct);

        await client.SendAsync(msg, ct);
        await client.DisconnectAsync(true, ct);
    }

    private static string StripHtml(string html)
    {
        try
        {
            var text = System.Text.RegularExpressions.Regex.Replace(html ?? "", "<.*?>", string.Empty);
            return string.IsNullOrWhiteSpace(text) ? html : text;
        }
        catch { return html; }
    }
}
