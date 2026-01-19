using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Dominio.Entidades;

namespace Aplicacao.Servicos;

public class AlocacaoServico
{
    private readonly IRepositorioCadeira _repositorioCadeira;
    private readonly IRepositorioAlocacao _repositorioAlocacao;

    public AlocacaoServico(IRepositorioCadeira repositorioCadeira, IRepositorioAlocacao repositorioAlocacao)
    {
        _repositorioCadeira = repositorioCadeira;
        _repositorioAlocacao = repositorioAlocacao;
    }

    public async Task<List<AlocacaoRespostaDto>> AlocarAutomaticamenteAsync(
        AlocacaoSolicitacaoDto solicitacao,
        CancellationToken cancellationToken)
    {
        var cadeiras = await _repositorioCadeira.ListarAsync(cancellationToken);

        if (cadeiras.Count == 0)
        {
            return new List<AlocacaoRespostaDto>();
        }

        var duracaoTotal = solicitacao.DataHoraFim - solicitacao.DataHoraInicio;
        var totalHoras = (int)Math.Ceiling(duracaoTotal.TotalHours);
        var intervalo = TimeSpan.FromHours(1);
        var alocacoes = new List<Alocacao>();

        for (var indice = 0; indice < totalHoras; indice++)
        {
            var cadeiraSelecionada = cadeiras[indice % cadeiras.Count];
            var inicio = solicitacao.DataHoraInicio.AddHours(indice);
            var fim = inicio.Add(intervalo);

            if (fim > solicitacao.DataHoraFim)
            {
                fim = solicitacao.DataHoraFim;
            }

            alocacoes.Add(new Alocacao
            {
                CadeiraId = cadeiraSelecionada.Id,
                DataHoraInicio = inicio,
                DataHoraFim = fim
            });
        }

        var persistidas = await _repositorioAlocacao.AdicionarEmLoteAsync(alocacoes, cancellationToken);

        return persistidas.Select(alocacao => new AlocacaoRespostaDto
        {
            Id = alocacao.Id,
            CadeiraId = alocacao.CadeiraId,
            NumeroCadeira = alocacao.Cadeira?.Numero ?? cadeiras.First(c => c.Id == alocacao.CadeiraId).Numero,
            DataHoraInicio = alocacao.DataHoraInicio,
            DataHoraFim = alocacao.DataHoraFim
        }).ToList();
    }
}
