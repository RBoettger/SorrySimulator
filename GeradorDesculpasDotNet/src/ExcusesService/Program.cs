using Microsoft.OpenApi.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExcusesService", Version = "v1" });
});

// HttpClient para o gerador Python
builder.Services.AddHttpClient("ExcuseGenerator", client =>
{
    client.BaseAddress = new Uri("http://excuse-generator:8083");
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Endpoint que chama o gerador Python
app.MapPost("/api/excuses/generate", async (
    IHttpClientFactory factory,  // 👈 corrigido aqui
    string nome,
    string? motivo
) =>
{
    var client = factory.CreateClient("ExcuseGenerator");

    var request = new
    {
        nome = nome,
        motivo = motivo
    };

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
.WithOpenApi(op =>
{
    op.Summary = "Gera uma desculpa automática usando o serviço Python";
    return op;
});

app.Run();
