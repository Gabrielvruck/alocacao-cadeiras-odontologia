using Dominio.Entidades;

namespace Aplicacao.Interfaces;

public interface IRepositorioCadeira
{
    Task<Cadeira> AdicionarAsync(Cadeira cadeira, CancellationToken cancellationToken);
    Task<Cadeira?> ObterPorIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Cadeira>> ListarAsync(CancellationToken cancellationToken);
    Task AtualizarAsync(Cadeira cadeira, CancellationToken cancellationToken);
    Task RemoverAsync(Cadeira cadeira, CancellationToken cancellationToken);
}
