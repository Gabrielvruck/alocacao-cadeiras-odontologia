using Dominio.Entidades;

namespace Aplicacao.Interfaces;

public interface IRepositorioCadeira
{
    Task<Cadeira> AdicionarAsync(Cadeira cadeira, CancellationToken cancellationToken);
    Task<Cadeira?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<Cadeira?> ObterPorNumeroAsync(int numero, CancellationToken cancellationToken);
    Task<List<Cadeira>> ListarAsync(CancellationToken cancellationToken);
    Task AtualizarAsync(Cadeira cadeira, CancellationToken cancellationToken);
    Task RemoverAsync(Cadeira cadeira, CancellationToken cancellationToken);

    /// <summary>
    /// Obtém alocações das cadeiras informadas dentro do período especificado.
    /// </summary>
    /// <param name="cadeiraIds">Ids das cadeiras a consultar.</param>
    /// <param name="inicio">Início do período (inclusive).</param>
    /// <param name="fim">Fim do período (exclusive).</param>
    Task<List<Alocacao>> ObterAlocacoesPorCadeirasNoPeriodoAsync(IEnumerable<int> cadeiraIds, DateTime inicio, DateTime fim, CancellationToken cancellationToken);
}
