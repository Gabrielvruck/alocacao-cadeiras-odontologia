using System.ComponentModel.DataAnnotations;

namespace Aplicacao.Modelos;

public class CadeiraCriacaoDto
{
    [Required(ErrorMessage = "O número é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "O número deve ser maior que zero.")]
    public int Numero { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
    public string Descricao { get; set; } = string.Empty;
}
