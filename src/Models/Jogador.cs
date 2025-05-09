using System.ComponentModel.DataAnnotations; // Adicione esta linha no início do arquivo

public class Jogador
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string Nome { get; set; }

    [Range(15, 50, ErrorMessage = "Idade deve ser entre 15 e 50 anos")]
    public int Idade { get; set; }

    // ... outras propriedades
}
