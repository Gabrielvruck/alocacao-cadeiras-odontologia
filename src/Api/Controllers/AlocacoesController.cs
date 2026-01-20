using Aplicacao.Modelos;
using Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlocacoesController(AlocacaoServico alocacaoServico) : ControllerBase
{
    private readonly AlocacaoServico _alocacaoServico = alocacaoServico;

    [HttpPost("automatica")]
    public async Task<ActionResult<List<AlocacaoRespostaDto>>> AlocarAutomaticamenteAsync(
        [FromBody] AlocacaoSolicitacaoDto solicitacao,
        CancellationToken cancellationToken)
    {
        var resultado = await _alocacaoServico.AlocarAutomaticamenteAsync(solicitacao, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet]
    public async Task<ActionResult<PaginacaoRespostaDto<AlocacaoRespostaDto>>> ListarPaginadoAsync([FromQuery] int? pageNumber, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        var pn = pageNumber.HasValue && pageNumber.Value > 0 ? pageNumber.Value : 1;
        var ps = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value : 10;

        var resultado = await _alocacaoServico.ListarPaginadoAsync(pn, ps, cancellationToken);
        return Ok(resultado);
    }
}
