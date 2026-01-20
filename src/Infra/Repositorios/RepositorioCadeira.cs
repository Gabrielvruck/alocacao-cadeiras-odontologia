using Aplicacao.Interfaces;
using Dominio.Entidades;
using Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios;

public class RepositorioCadeira(AplicacaoDbContext contexto) : IRepositorioCadeira
{
    private readonly AplicacaoDbContext _contexto = contexto;

    public async Task<Cadeira> AdicionarAsync(Cadeira cadeira, CancellationToken cancellationToken)
    {
        _contexto.Cadeiras.Add(cadeira);
        await _contexto.SaveChangesAsync(cancellationToken);
        return cadeira;
    }

    public async Task<Cadeira?> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _contexto.Cadeiras
            .AsNoTracking()
            .Include(c => c.Alocacoes)
            .FirstOrDefaultAsync(cadeira => cadeira.Id == id, cancellationToken);
    }

    public async Task<Cadeira?> ObterPorNumeroAsync(int numero, CancellationToken cancellationToken)
    {
        return await _contexto.Cadeiras
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Numero == numero, cancellationToken);
    }

    public async Task<List<Cadeira>> ListarAsync(CancellationToken cancellationToken)
    {
        return await _contexto.Cadeiras
            .AsNoTracking()
            .OrderBy(cadeira => cadeira.Numero)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Recupera alocações para um conjunto de cadeiras dentro de um período.
    /// Usado para evitar consultas N+1 ao verificar conflitos de horário.
    /// </summary>
    public async Task<List<Alocacao>> ObterAlocacoesPorCadeirasNoPeriodoAsync(IEnumerable<int> cadeiraIds, DateTime inicio, DateTime fim, CancellationToken cancellationToken)
    {
        var ids = cadeiraIds.ToList();
        if (ids.Count == 0) return new List<Alocacao>();

        return await _contexto.Alocacoes
            .AsNoTracking()
            .Where(a => ids.Contains(a.CadeiraId) && a.DataHoraInicio < fim && a.DataHoraFim > inicio)
            .ToListAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Cadeira cadeira, CancellationToken cancellationToken)
    {
        _contexto.Cadeiras.Update(cadeira);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Cadeira cadeira, CancellationToken cancellationToken)
    {
        _contexto.Cadeiras.Remove(cadeira);
        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
