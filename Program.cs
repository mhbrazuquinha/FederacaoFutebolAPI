using Microsoft.EntityFrameworkCore;
using FederacaoFutebolApi.Data;
using FederacaoFutebolApi.Models;

var  myAllowSpecificOrigins = "_myAllowSpecificOrigins"; //politica de CORS

var builder = WebApplication.CreateBuilder(args);

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins(
                                "null"                    
                               )
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                      });
});


// config banco de dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=federacao.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(myAllowSpecificOrigins);



//ENDPOINTS  TIMES

//Criar um novo time
app.MapPost("/times", async (AppDbContext dbContext, Time time) =>
{
    if (time == null)
    {
        return Results.BadRequest("Dados do time inválidos.");
    }
    if (string.IsNullOrWhiteSpace(time.Nome))
    {
        return Results.BadRequest("O nome do time é obrigatório.");
    }
    dbContext.Times.Add(time);
    await dbContext.SaveChangesAsync();
    return Results.CreatedAtRoute("GetTimeById", new { id = time.Id }, time);
});

//Lista os times
app.MapGet("/times", async (AppDbContext dbContext) =>
{
    var times = await dbContext.Times.Include(t => t.Jogadores).ToListAsync();
    return Results.Ok(times);
});

//Busca times
app.MapGet("/times/{id:int}", async (AppDbContext dbContext, int id) =>
{
    var time = await dbContext.Times.Include(t => t.Jogadores).FirstOrDefaultAsync(t => t.Id == id);
    if (time == null)
    {
        return Results.NotFound($"Time com ID {id} não encontrado.");
    }
    return Results.Ok(time);
}).WithName("GetTimeById");

//Lista os jogadores do time
app.MapGet("/times/{timeId:int}/jogadores", async (AppDbContext dbContext, int timeId) =>
{
    var time = await dbContext.Times.FindAsync(timeId);
    if (time == null)
    {
        return Results.NotFound($"Time com ID {timeId} não encontrado.");
    }
    var jogadoresDoTime = await dbContext.Jogadores
                                    .Where(j => j.TimeId == timeId)
                                    .ToListAsync();
    return Results.Ok(jogadoresDoTime);
});

//Atualiza um time
app.MapPut("/times/{id:int}", async (AppDbContext dbContext, int id, Time timeAtualizado) =>
{
    if (timeAtualizado == null || id != timeAtualizado.Id)
    {
        return Results.BadRequest("Dados do time inválidos ou IDs inconsistentes.");
    }
    var timeExistente = await dbContext.Times.FindAsync(id);
    if (timeExistente == null)
    {
        return Results.NotFound($"Time com ID {id} não encontrado para atualização.");
    }
    timeExistente.Nome = timeAtualizado.Nome;
    timeExistente.DataCriacao = timeAtualizado.DataCriacao;
    timeExistente.CidadeOrigem = timeAtualizado.CidadeOrigem;
    timeExistente.UrlEscudo = timeAtualizado.UrlEscudo;
    try
    {
        await dbContext.SaveChangesAsync();
        return Results.Ok(timeExistente);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict("Ocorreu um conflito de concorrência ao salvar o time. Tente novamente.");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocorreu um erro ao atualizar o time: {ex.Message}");
    }
});

//Deletar um time
app.MapDelete("/times/{id:int}", async (AppDbContext dbContext, int id) =>
{
    var time = await dbContext.Times.FindAsync(id);
    if (time == null)
    {
        return Results.NotFound($"Time com ID {id} não encontrado para exclusão.");
    }
    dbContext.Times.Remove(time);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

//ENDPOINTS PARA TIMES

//ENDPOINTSPARA JOGADORES

//Criar um jogador
app.MapPost("/jogadores", async (AppDbContext dbContext, Jogador jogador) =>
{
    if (jogador == null)
    {
        return Results.BadRequest("Dados do jogador inválidos.");
    }
    if (string.IsNullOrWhiteSpace(jogador.Nome))
    {
        return Results.BadRequest("O nome do jogador é obrigatório.");
    }
    var timeExistente = await dbContext.Times.FindAsync(jogador.TimeId);
    if (timeExistente == null)
    {
        return Results.BadRequest($"Time com ID {jogador.TimeId} não encontrado. Não é possível adicionar o jogador.");
    }
    dbContext.Jogadores.Add(jogador);
    await dbContext.SaveChangesAsync();
    await dbContext.Entry(jogador).Reference(j => j.Time).LoadAsync();
    return Results.CreatedAtRoute("GetJogadorById", new { id = jogador.Id }, jogador);
});

//Listar os jogadores
app.MapGet("/jogadores", async (AppDbContext dbContext) =>
{
    var jogadores = await dbContext.Jogadores.Include(j => j.Time).ToListAsync();
    return Results.Ok(jogadores);
});

//Chama um jogador pelo id
app.MapGet("/jogadores/{id:int}", async (AppDbContext dbContext, int id) =>
{
    var jogador = await dbContext.Jogadores.Include(j => j.Time).FirstOrDefaultAsync(j => j.Id == id);
    if (jogador == null)
    {
        return Results.NotFound($"Jogador com ID {id} não encontrado.");
    }
    return Results.Ok(jogador);
}).WithName("GetJogadorById");

//Atualizar um jogador
app.MapPut("/jogadores/{id:int}", async (AppDbContext dbContext, int id, Jogador jogadorAtualizado) =>
{
    if (jogadorAtualizado == null || id != jogadorAtualizado.Id)
    {
        return Results.BadRequest("Dados do jogador inválidos ou IDs inconsistentes.");
    }
    var jogadorExistente = await dbContext.Jogadores.FindAsync(id);
    if (jogadorExistente == null)
    {
        return Results.NotFound($"Jogador com ID {id} não encontrado para atualização.");
    }
    if (jogadorExistente.TimeId != jogadorAtualizado.TimeId)
    {
        var timeNovoExistente = await dbContext.Times.FindAsync(jogadorAtualizado.TimeId);
        if (timeNovoExistente == null)
        {
            return Results.BadRequest($"Novo Time com ID {jogadorAtualizado.TimeId} não encontrado. Não é possível atualizar o time do jogador.");
        }
    }
    jogadorExistente.Nome = jogadorAtualizado.Nome;
    jogadorExistente.TimeId = jogadorAtualizado.TimeId;
    try
    {
        await dbContext.SaveChangesAsync();
        await dbContext.Entry(jogadorExistente).Reference(j => j.Time).LoadAsync();
        return Results.Ok(jogadorExistente);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict("Ocorreu um conflito de concorrência ao salvar o jogador.");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ocorreu um erro ao atualizar o jogador: {ex.Message}");
    }
});

//Deleta um jogador
app.MapDelete("/jogadores/{id:int}", async (AppDbContext dbContext, int id) =>
{
    var jogador = await dbContext.Jogadores.FindAsync(id);
    if (jogador == null)
    {
        return Results.NotFound($"Jogador com ID {id} não encontrado para exclusão.");
    }
    dbContext.Jogadores.Remove(jogador);
    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

//ENDPOINTS PARA JOGADORES

// verifica se a api está de pé
app.MapGet("/", () => "Bem-vindo à API da Federação de Futebol!");

app.Run();