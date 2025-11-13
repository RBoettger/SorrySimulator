using Microsoft.OpenApi.Models;
using System.Net.Http;
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
    string nome,
    string? motivo
) =>
{
    var client = factory.CreateClient("ExcuseGenerator");

    var request = new { nome, motivo };

    try
    {
        var response = await client.PostAsJsonAsync("/gerar", request);
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
.WithDescription("Envia o nome e o motivo para o gerador Python e retorna a desculpa gerada.");

app.Run();
