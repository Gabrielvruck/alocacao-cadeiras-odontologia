using System.ComponentModel.DataAnnotations;

namespace Aplicacao.Modelos;

public class AlocacaoSolicitacaoDto
{
    /// <summary>
    /// Data e hora de início do período a ser alocado.
    /// </summary>
    [Required(ErrorMessage = "A data/hora de início é obrigatória.")]
    public DateTime DataHoraInicio { get; set; }

    /// <summary>
    /// Data e hora de fim do período a ser alocado.
    /// </summary>
    [Required(ErrorMessage = "A data/hora de fim é obrigatória.")]
    public DateTime DataHoraFim { get; set; }
}
