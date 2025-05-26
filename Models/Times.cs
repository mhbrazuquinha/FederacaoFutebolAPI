using System.ComponentModel.DataAnnotations;

namespace FederacaoFutebolApi.Models
{
    public class Time
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do time é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome do time deve ter entre 3 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [StringLength(100, ErrorMessage = "A cidade de origem deve ter no máximo 100 caracteres.")]
        public string? CidadeOrigem { get; set; }
        [Url(ErrorMessage = "A URL do escudo deve ser uma URL válida.")]
        public string? UrlEscudo { get; set; }

        public virtual ICollection<Jogador> Jogadores { get; set; } = new List<Jogador>();
    }
}