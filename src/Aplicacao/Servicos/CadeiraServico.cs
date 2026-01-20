using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Dominio.Entidades;

namespace Aplicacao.Servicos;

public class CadeiraServico(IRepositorioCadeira repositorioCadeira)
{
    private readonly IRepositorioCadeira _repositorioCadeira = repositorioCadeira;

    public async Task<CadeiraRespostaDto> CriarAsync(CadeiraCriacaoDto dto, CancellationToken cancellationToken)
    {
        var cadeira = new Cadeira
        {
            Numero = dto.Numero,
            Descricao = dto.Descricao
        };

        var criada = await _repositorioCadeira.AdicionarAsync(cadeira, cancellationToken);

        return new CadeiraRespostaDto
        {
            Id = criada.Id,
            Numero = criada.Numero,
            Descricao = criada.Descricao
        };
    }

    public async Task<PaginacaoRespostaDto<CadeiraRespostaDto>> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var cadeiras = (await _repositorioCadeira.ListarAsync(cancellationToken))
            .OrderBy(c => c.Numero)
            .ToList();

        var total = cadeiras.Count;

        var pageItems = cadeiras
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(cadeira => new CadeiraRespostaDto
            {
                Id = cadeira.Id,
                Numero = cadeira.Numero,
                Descricao = cadeira.Descricao
            })
            .ToList();

        return new PaginacaoRespostaDto<CadeiraRespostaDto>
        {
            Items = pageItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<List<CadeiraRespostaDto>> ListarAsync(CancellationToken cancellationToken)
    {
        var cadeiras = await _repositorioCadeira.ListarAsync(cancellationToken);

        return cadeiras.Select(cadeira => new CadeiraRespostaDto
        {
            Id = cadeira.Id,
            Numero = cadeira.Numero,
            Descricao = cadeira.Descricao
        }).ToList();
    }

    public async Task<CadeiraRespostaDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var cadeira = await _repositorioCadeira.ObterPorIdAsync(id, cancellationToken);

        if (cadeira is null)
        {
            return null;
        }

        return new CadeiraRespostaDto
        {
            Id = cadeira.Id,
            Numero = cadeira.Numero,
            Descricao = cadeira.Descricao
        };
    }

    public async Task<bool> AtualizarAsync(int id, CadeiraAtualizacaoDto dto, CancellationToken cancellationToken)
    {
        var cadeira = await _repositorioCadeira.ObterPorIdAsync(id, cancellationToken);

        if (cadeira is null)
        {
            return false;
        }

        cadeira.Numero = dto.Numero;
        cadeira.Descricao = dto.Descricao;

        await _repositorioCadeira.AtualizarAsync(cadeira, cancellationToken);
        return true;
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var cadeira = await _repositorioCadeira.ObterPorIdAsync(id, cancellationToken);

        if (cadeira is null)
        {
            return false;
        }

        await _repositorioCadeira.RemoverAsync(cadeira, cancellationToken);
        return true;
    }
}
