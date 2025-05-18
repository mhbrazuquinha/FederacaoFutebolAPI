using FedercaoFutebolAPI.Data;
using FedercaoFutebolAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração mínima do CORS
builder.Services.AddCors(options => 
{
    options.AddPolicy("PermitirTudo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configuração do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Habilitar CORS
app.UseCors("PermitirTudo");

// --- ENDPOINTS PRINCIPAIS ---

// GET /times - Listar todos os times
app.MapGet("/times", async (AppDbContext db) => 
    await db.Times.ToListAsync());

// GET /times/{id} - Buscar time por ID
app.MapGet("/times/{id}", async (int id, AppDbContext db) =>
    await db.Times.FindAsync(id) is Time time 
        ? Results.Ok(time) 
        : Results.NotFound());

// POST /times - Criar novo time
app.MapPost("/times", async (Time time, AppDbContext db) =>
{
    db.Times.Add(time);
    await db.SaveChangesAsync();
    return Results.Created($"/times/{time.Id}", time);
});

// PUT /times/{id} - Atualizar time
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

// DELETE /times/{id} - Remover time
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

// GET /times/{timeId}/jogadores - Listar jogadores de um time
app.MapGet("/times/{timeId}/jogadores", async (int timeId, AppDbContext db) =>
{
    if (!await db.Times.AnyAsync(t => t.Id == timeId))
        return Results.NotFound("Time não encontrado");

    return Results.Ok(await db.Jogadores.Where(j => j.TimeId == timeId).ToListAsync());
});

app.Run();