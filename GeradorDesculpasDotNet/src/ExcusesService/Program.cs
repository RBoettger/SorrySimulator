using Microsoft.OpenApi.Models;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ExcusesService",
        Version = "v1",
        Description = "Serviço que gera desculpas automáticas chamando o gerador Python."
    });
});

builder.Services.AddHttpClient("ExcuseGenerator", client =>
{
    client.BaseAddress = new Uri("http://excuse-generator:8083");
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/excuses/generate", async (
    IHttpClientFactory factory,
    ExcuseRequest request
) =>
{
    var client = factory.CreateClient("ExcuseGenerator");

    var payload = new
    {
        nome = request.Nome,
        motivo = request.Motivo,
        tom = request.Tom
    };

    try
    {
        var response = await client.PostAsJsonAsync("/gerar", payload);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<object>();
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao gerar desculpa: {ex.Message}");
    }
})
.WithName("GerarDesculpa")
.WithSummary("Gera uma desculpa automática usando o serviço Python.")
.WithDescription("Envia o nome, motivo e tom para o gerador Python e retorna a desculpa gerada.");

app.Run();

public record ExcuseRequest(string Nome, string? Motivo, string? Tom);