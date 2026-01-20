using Aplicacao.Interfaces;
using Dominio.Entidades;
using Infra.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios;

public class RepositorioAlocacao(AplicacaoDbContext contexto) : IRepositorioAlocacao
{
    private readonly AplicacaoDbContext _contexto = contexto;

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

    public async Task<(List<Alocacao> Items, int Total)> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _contexto.Alocacoes
            .AsNoTracking()
            .Include(a => a.Cadeira)
            .OrderBy(a => a.DataHoraInicio);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
