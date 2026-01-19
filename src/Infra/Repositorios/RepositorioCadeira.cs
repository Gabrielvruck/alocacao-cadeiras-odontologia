using Aplicacao.Interfaces;
using Dominio.Entidades;
using Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios;

public class RepositorioCadeira : IRepositorioCadeira
{
    private readonly AplicacaoDbContext _contexto;

    public RepositorioCadeira(AplicacaoDbContext contexto)
    {
        _contexto = contexto;
    }

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
            .FirstOrDefaultAsync(cadeira => cadeira.Id == id, cancellationToken);
    }

    public async Task<List<Cadeira>> ListarAsync(CancellationToken cancellationToken)
    {
        return await _contexto.Cadeiras
            .AsNoTracking()
            .OrderBy(cadeira => cadeira.Numero)
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
