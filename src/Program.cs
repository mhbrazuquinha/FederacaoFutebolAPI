using FedercaoFutebolAPI.Data;
using FedercaoFutebolAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;  // adicionado para o uso do Swagger

var builder = WebApplication.CreateBuilder(args);

// Configuração do SQLite (já existente)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>  // Modificado para incluir configuração
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Federação Futebol API", 
        Version = "v1",
        Description = "API para gerenciamento de times e jogadores" 
    });
});

var app = builder.Build();

// --- NOVOS ENDPOINTS PARA TIMES ---
app.MapGet("/times", async (AppDbContext db) => 
    await db.Times.ToListAsync());

app.MapGet("/times/{id}", async (int id, AppDbContext db) =>
    await db.Times.FindAsync(id) is Time time 
        ? Results.Ok(time) 
        : Results.NotFound());

app.MapPost("/times", async (Time time, AppDbContext db) =>
{
    db.Times.Add(time);
    await db.SaveChangesAsync();
    return Results.Created($"/times/{time.Id}", time);
});

app.MapPut("/times/{id}", async (int id, Time inputTime, AppDbContext db) =>
{
    var time = await db.Times.FindAsync(id);
    if (time is null) return Results.NotFound();
    
    time.Nome = inputTime.Nome;
    time.CidadeOrigem = inputTime.CidadeOrigem;
    time.EscudoUrl = inputTime.EscudoUrl;
    time.Titulos = inputTime.Titulos;
    
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/times/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Times.FindAsync(id) is Time time)
    {
        db.Times.Remove(time);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>  // Configuração adicional
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Futebol API v1");
        c.DocumentTitle = "Documentação da API";
    });
}
//IMPLEMENTAÇÃO DE SEGURANÇA 


app.Run();