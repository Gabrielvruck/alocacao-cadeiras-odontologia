using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Dominio.Entidades;

namespace Aplicacao.Servicos;

public class CadeiraServico
{
    private readonly IRepositorioCadeira _repositorioCadeira;

    public CadeiraServico(IRepositorioCadeira repositorioCadeira)
    {
        _repositorioCadeira = repositorioCadeira;
    }

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

    public async Task<bool> AtualizarAsync(int id, CadeiraCriacaoDto dto, CancellationToken cancellationToken)
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
