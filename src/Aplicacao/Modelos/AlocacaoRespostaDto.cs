namespace Aplicacao.Modelos;

public class AlocacaoRespostaDto
{
    public int Id { get; set; }
    public int CadeiraId { get; set; }
    public int NumeroCadeira { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
}
