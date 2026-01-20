using Aplicacao.Modelos;
using Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller responsável por operações relacionadas às alocações de cadeiras.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AlocacoesController(AlocacaoServico alocacaoServico) : ControllerBase
{
    private readonly AlocacaoServico _alocacaoServico = alocacaoServico;

    [HttpPost("automatica")]
    /// <summary>
    /// Cria alocações automaticamente no intervalo informado, distribuindo entre cadeiras disponíveis.
    /// </summary>
    /// <param name="solicitacao">Intervalo de início e fim para geração das alocações.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Lista de alocações criadas.</returns>
    public async Task<ActionResult<List<AlocacaoRespostaDto>>> AlocarAutomaticamenteAsync(
        [FromBody] AlocacaoSolicitacaoDto solicitacao,
        CancellationToken cancellationToken)
    {
        var resultado = await _alocacaoServico.AlocarAutomaticamenteAsync(solicitacao, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet]
    /// <summary>
    /// Retorna as alocações paginadas.
    /// </summary>
    /// <param name="pageNumber">Número da página (1-based).</param>
    /// <param name="pageSize">Tamanho da página.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>DTO de paginação com itens e metadados.</returns>
    public async Task<ActionResult<PaginacaoRespostaDto<AlocacaoRespostaDto>>> ListarPaginadoAsync([FromQuery] int? pageNumber, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var pn = pageNumber.HasValue && pageNumber.Value > 0 ? pageNumber.Value : 1;
        var ps = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value : 10;

        var resultado = await _alocacaoServico.ListarPaginadoAsync(pn, ps, cancellationToken);
        return Ok(resultado);
    }
}
