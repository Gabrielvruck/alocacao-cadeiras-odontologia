using Aplicacao.Modelos;
using Aplicacao.Servicos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller responsável por operações CRUD sobre cadeiras.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CadeirasController(CadeiraServico cadeiraServico) : ControllerBase
{
    private readonly CadeiraServico _cadeiraServico = cadeiraServico;

    [HttpGet]
    /// <summary>
    /// Lista todas as cadeiras ou retorna página específica quando parâmetros são informados.
    /// </summary>
    /// <param name="pageNumber">Número da página (opcional).</param>
    /// <param name="pageSize">Tamanho da página (opcional).</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    public async Task<ActionResult> ListarAsync([FromQuery] int? pageNumber, [FromQuery] int? pageSize, CancellationToken cancellationToken)
    {
        if (pageNumber.HasValue || pageSize.HasValue)
        {
            var pag = await _cadeiraServico.ListarPaginadoAsync(pageNumber ?? 1, pageSize ?? 10, cancellationToken);
            return Ok(pag);
        }

        var resultado = await _cadeiraServico.ListarAsync(cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:int}", Name = "Cadeiras_ObterPorId")]
    /// <summary>
    /// Obtém uma cadeira por seu identificador.
    /// </summary>
    /// <param name="id">Identificador da cadeira.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>DTO da cadeira ou NotFound se não existir.</returns>
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
    /// <summary>
    /// Cria uma nova cadeira.
    /// </summary>
    /// <param name="dto">Dados de criação da cadeira.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>Cadeira criada com identificador.</returns>
    public async Task<ActionResult<CadeiraRespostaDto>> CriarAsync(
        [FromBody] CadeiraCriacaoDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _cadeiraServico.CriarAsync(dto, cancellationToken);
        return CreatedAtRoute("Cadeiras_ObterPorId", new { id = resultado.Id }, resultado);
    }

    [HttpPut("{id:int}")]
    /// <summary>
    /// Atualiza uma cadeira existente.
    /// </summary>
    /// <param name="id">Identificador da cadeira a ser atualizada.</param>
    /// <param name="dto">Dados de atualização.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>NoContent se atualizado; NotFound se não existir.</returns>
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
    /// <summary>
    /// Remove uma cadeira pelo id.
    /// </summary>
    /// <param name="id">Identificador da cadeira a ser removida.</param>
    /// <param name="cancellationToken">Token para cancelar a operação.</param>
    /// <returns>NoContent se removido; NotFound se não existir.</returns>
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
