using Dominio.Entidades;

namespace Aplicacao.Interfaces;

public interface IRepositorioAlocacao
{
    Task<List<Alocacao>> AdicionarEmLoteAsync(List<Alocacao> alocacoes, CancellationToken cancellationToken);
    Task<(List<Alocacao> Items, int Total)> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
