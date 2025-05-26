using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FederacaoFutebolApi.Models
{
    public class Jogador
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do jogador é obrigatório.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome do jogador deve ter entre 2 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;
        public int TimeId { get; set; }
        [ForeignKey("TimeId")]
        public virtual Time? Time { get; set; }
    }
}