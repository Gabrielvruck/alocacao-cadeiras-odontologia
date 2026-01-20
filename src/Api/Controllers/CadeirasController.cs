using Aplicacao.Modelos;
using Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CadeirasController(CadeiraServico cadeiraServico) : ControllerBase
{
    private readonly CadeiraServico _cadeiraServico = cadeiraServico;

    [HttpGet]
    public async Task<ActionResult<List<CadeiraRespostaDto>>> ListarAsync(CancellationToken cancellationToken)
    {
        var resultado = await _cadeiraServico.ListarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:int}", Name = "Cadeiras_ObterPorId")]
    public async Task<ActionResult<CadeiraRespostaDto>> ObterPorIdAsync(int id, CancellationToken cancellationToken)
    {
        var resultado = await _cadeiraServico.ObterPorIdAsync(id, cancellationToken);

        if (resultado is null)
        {
            return NotFound();
        }

        return Ok(resultado);
    }

    [HttpPost]
    public async Task<ActionResult<CadeiraRespostaDto>> CriarAsync(
        [FromBody] CadeiraCriacaoDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _cadeiraServico.CriarAsync(dto, cancellationToken);
        return CreatedAtRoute("Cadeiras_ObterPorId", new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> AtualizarAsync(
        int id,
        [FromBody] CadeiraAtualizacaoDto dto,
        CancellationToken cancellationToken)
    {
        var atualizado = await _cadeiraServico.AtualizarAsync(id, dto, cancellationToken);

        if (!atualizado)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoverAsync(int id, CancellationToken cancellationToken)
    {
        var removido = await _cadeiraServico.RemoverAsync(id, cancellationToken);

        if (!removido)
        {
            return NotFound();
        }

        return NoContent();
    }
}
