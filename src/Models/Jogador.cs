// Models/Jogador.cs
using System.ComponentModel.DataAnnotations;

namespace FedercaoFutebolAPI.Models;

public class Jogador
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Range(15, 50, ErrorMessage = "Idade deve ser entre 15 e 50 anos")]
    public int Idade { get; set; }

    // Relacionamento com Time
    public int? TimeId { get; set; }
    public Time? Time { get; set; }
    
    // Adicione mais propriedades conforme necessário
    public string? Posicao { get; set; }
    public int NumeroCamisa { get; set; }
}