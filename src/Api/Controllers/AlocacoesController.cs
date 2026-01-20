using Aplicacao.Modelos;
using Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/alocacoes")]
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
}
