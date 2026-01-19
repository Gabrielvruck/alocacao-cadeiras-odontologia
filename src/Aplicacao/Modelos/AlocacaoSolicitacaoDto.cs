using System.ComponentModel.DataAnnotations;

namespace Aplicacao.Modelos;

public class AlocacaoSolicitacaoDto
{
    [Required(ErrorMessage = "A data/hora de início é obrigatória.")]
    public DateTime DataHoraInicio { get; set; }

    [Required(ErrorMessage = "A data/hora de fim é obrigatória.")]
    public DateTime DataHoraFim { get; set; }
}
