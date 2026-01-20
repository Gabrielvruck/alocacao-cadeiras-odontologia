using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Dominio.Entidades;

namespace Aplicacao.Servicos;

public class CadeiraServico(IRepositorioCadeira repositorioCadeira)
{
    private readonly IRepositorioCadeira _repositorioCadeira = repositorioCadeira;

    /// <summary>
    /// Cria uma cadeira e retorna seu DTO de resposta.
    /// </summary>
    /// <param name="dto">Dados para criação da cadeira.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>DTO com os dados da cadeira criada.</returns>
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

    /// <summary>
    /// Retorna lista paginada de cadeiras com metadados de paginação.
    /// </summary>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>DTO de paginação com itens e metadados.</returns>
    public async Task<PaginacaoRespostaDto<CadeiraRespostaDto>> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        // normaliza parâmetros de paginação
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

    /// <summary>
    /// Lista todas as cadeiras sem paginação.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Lista de DTOs de cadeira.</returns>
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

    /// <summary>
    /// Obtém uma cadeira por id.
    /// </summary>
    /// <param name="id">Identificador da cadeira.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>DTO da cadeira ou null se não existir.</returns>
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

    /// <summary>
    /// Atualiza uma cadeira existente.
    /// </summary>
    /// <param name="id">Identificador da cadeira.</param>
    /// <param name="dto">Dados de atualização.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>True se atualizado; false se a cadeira não existir.</returns>
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

    /// <summary>
    /// Remove uma cadeira pelo id.
    /// </summary>
    /// <param name="id">Identificador da cadeira.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>True se removido; false se não existir.</returns>
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
