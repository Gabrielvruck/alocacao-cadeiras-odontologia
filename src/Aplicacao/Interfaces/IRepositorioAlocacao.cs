using Dominio.Entidades;

namespace Aplicacao.Interfaces;

public interface IRepositorioAlocacao
{
    Task<List<Alocacao>> AdicionarEmLoteAsync(List<Alocacao> alocacoes, CancellationToken cancellationToken);
}
