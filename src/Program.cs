using FedercaoFutebolAPI.Data;
using FedercaoFutebolAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});



// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Federação Futebol API", 
        Version = "v1",
        Description = "API para gerenciamento de times e jogadores" 
    });
});

var app = builder.Build();

// Configuração do Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Futebol API v1");
        c.DocumentTitle = "Documentação da API";
    });
}

// --- ENDPOINTS PARA JOGADORES ---
app.MapGet("/jogadores", async (AppDbContext db) => 
    await db.Jogadores.Include(j => j.Time).ToListAsync());

app.MapGet("/jogadores/{id}", async (int id, AppDbContext db) =>
    await db.Jogadores.Include(j => j.Time).FirstOrDefaultAsync(j => j.Id == id) is Jogador jogador
        ? Results.Ok(jogador)
        : Results.NotFound());

app.MapPost("/jogadores", async (Jogador jogador, AppDbContext db) =>
{
    if (jogador.TimeId.HasValue && !await db.Times.AnyAsync(t => t.Id == jogador.TimeId.Value))
        return Results.BadRequest("Time não encontrado");

    db.Jogadores.Add(jogador);
    await db.SaveChangesAsync();
    return Results.Created($"/jogadores/{jogador.Id}", jogador);
});

app.MapPut("/jogadores/{id}", async (int id, Jogador inputJogador, AppDbContext db) =>
{
    var jogador = await db.Jogadores.FindAsync(id);
    if (jogador is null) return Results.NotFound();

    if (inputJogador.TimeId.HasValue && !await db.Times.AnyAsync(t => t.Id == inputJogador.TimeId.Value))
        return Results.BadRequest("Time não encontrado");

    jogador.Nome = inputJogador.Nome;
    jogador.Idade = inputJogador.Idade;
    jogador.Posicao = inputJogador.Posicao;
    jogador.NumeroCamisa = inputJogador.NumeroCamisa;
    jogador.TimeId = inputJogador.TimeId;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/jogadores/{id}", async (int id, AppDbContext db) =>
{
    if (await db.Jogadores.FindAsync(id) is Jogador jogador)
    {
        db.Jogadores.Remove(jogador);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

// --- ENDPOINTS PARA TIMES ---
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

// --- ENDPOINTS ADICIONAIS ---
app.MapGet("/times/{timeId}/jogadores", async (int timeId, AppDbContext db) =>
{
    if (!await db.Times.AnyAsync(t => t.Id == timeId))
        return Results.NotFound("Time não encontrado");

    return Results.Ok(await db.Jogadores.Where(j => j.TimeId == timeId).ToListAsync());
});

app.MapGet("/populate", async (AppDbContext db) =>
{
    if (await db.Times.AnyAsync()) 
        return Results.BadRequest("Banco já possui dados");

    var time1 = new Time { Nome = "Flamengo", CidadeOrigem = "Rio de Janeiro" };
    var time2 = new Time { Nome = "Palmeiras", CidadeOrigem = "São Paulo" };
    
    db.Times.AddRange(time1, time2);
    await db.SaveChangesAsync();

    db.Jogadores.AddRange(
        new Jogador { Nome = "Gabigol", Idade = 27, TimeId = time1.Id, Posicao = "Atacante", NumeroCamisa = 9 },
        new Jogador { Nome = "Arrascaeta", Idade = 28, TimeId = time1.Id, Posicao = "Meia", NumeroCamisa = 14 },
        new Jogador { Nome = "Dudu", Idade = 31, TimeId = time2.Id, Posicao = "Atacante", NumeroCamisa = 7 }
    );
    
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Dados de teste criados!", times = new[] { time1, time2 } });
});

app.MapGet("/test-db", async (AppDbContext db) =>
{
    try 
    {
        var timesCount = await db.Times.CountAsync();
        return Results.Ok($"Banco funcionando! Times cadastrados: {timesCount}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Erro ao acessar o banco: {ex.Message}");
    }
});

app.Run();