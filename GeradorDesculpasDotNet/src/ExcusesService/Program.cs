using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExcusesService", Version = "v1" });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/excuses/generate", (string? motivo, string? tom) =>
{
    var tone = string.IsNullOrWhiteSpace(tom) ? "profissional" : tom;
    var reason = string.IsNullOrWhiteSpace(motivo) ? "imprevistos pessoais" : motivo;
    var excuse = $"Olá, infelizmente precisei ajustar minha agenda hoje devido a {reason}. Já reorganizei minhas entregas e vou compensar as horas, mantendo o comprometimento com os prazos. Agradeço a compreensão.";
    if ((tone ?? "").ToLowerInvariant() == "informal")
        excuse = $"Foi mal! Tive um perrengue com {reason} e não vou conseguir agora. Vou repor e deixar tudo em dia. Valeu pela compreensão!";
    return Results.Ok(new { message = excuse, tone = tone, reason = reason });
});

app.Run();