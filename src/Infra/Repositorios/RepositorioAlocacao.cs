using Aplicacao.Interfaces;
using Dominio.Entidades;
using Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios;

public class RepositorioAlocacao : IRepositorioAlocacao
{
    private readonly AplicacaoDbContext _contexto;

    public RepositorioAlocacao(AplicacaoDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<List<Alocacao>> AdicionarEmLoteAsync(List<Alocacao> alocacoes, CancellationToken cancellationToken)
    {
        _contexto.Alocacoes.AddRange(alocacoes);
        await _contexto.SaveChangesAsync(cancellationToken);

        var ids = alocacoes.Select(alocacao => alocacao.Id).ToList();

        var carregadas = await _contexto.Alocacoes
            .Include(alocacao => alocacao.Cadeira)
            .Where(alocacao => ids.Contains(alocacao.Id))
            .ToListAsync(cancellationToken);

        // Preserve the original order of the input list
        var mapa = carregadas.ToDictionary(a => a.Id);
        return [.. alocacoes.Select(a => mapa[a.Id])];
    }
}
