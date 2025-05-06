namespace FedercaoFutebolAPI.Models;

public class Time
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public string? CidadeOrigem { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public string? EscudoUrl { get; set; }
    public int Titulos { get; set; } = 0;
}
