
using System.ComponentModel.DataAnnotations;

namespace FedercaoFutebolAPI.Models;

public class Time
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Nome do time é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public required string Nome { get; set; }
    
    public string? CidadeOrigem { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public string? EscudoUrl { get; set; }
    public int Titulos { get; set; } = 0;
    
    // Relacionamento com Jogadores
    public List<Jogador>? Jogadores { get; set; }
}