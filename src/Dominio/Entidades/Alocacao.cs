namespace Dominio.Entidades;

public class Alocacao
{
    public int Id { get; set; }
    public int CadeiraId { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public Cadeira? Cadeira { get; set; }
}
