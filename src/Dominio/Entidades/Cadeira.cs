namespace Dominio.Entidades;

public class Cadeira
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public virtual List<Alocacao> Alocacoes { get; set; } = new();
}
