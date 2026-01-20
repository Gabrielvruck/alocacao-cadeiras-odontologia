using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Dominio.Entidades;

namespace Aplicacao.Servicos;

public class AlocacaoServico(IRepositorioCadeira repositorioCadeira, IRepositorioAlocacao repositorioAlocacao)
{
    private readonly IRepositorioCadeira _repositorioCadeira = repositorioCadeira;
    private readonly IRepositorioAlocacao _repositorioAlocacao = repositorioAlocacao;

    public async Task<List<AlocacaoRespostaDto>> AlocarAutomaticamenteAsync(
        AlocacaoSolicitacaoDto solicitacao,
        CancellationToken cancellationToken)
    {
        var cadeiras = (await _repositorioCadeira.ListarAsync(cancellationToken))
            .OrderBy(c => c.Numero)
            .ToList();

        if (cadeiras.Count == 0)
        {
            return [];
        }

        // DEBUG: show ordered chairs
        System.Console.WriteLine("[DEBUG] cadeiras order: " + string.Join(",", cadeiras.Select(c => c.Numero)));

        var duracaoTotal = solicitacao.DataHoraFim - solicitacao.DataHoraInicio;
        var totalHoras = (int)Math.Ceiling(duracaoTotal.TotalHours);
        var intervalo = TimeSpan.FromHours(1);
        var alocacoes = new List<Alocacao>();
        var respostasParciais = new List<AlocacaoRespostaDto>();

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

            respostasParciais.Add(new AlocacaoRespostaDto
            {
                Id = 0,
                CadeiraId = cadeiraSelecionada.Id,
                NumeroCadeira = cadeiraSelecionada.Numero,
                DataHoraInicio = inicio,
                DataHoraFim = fim
            });
        }

        var persistidas = await _repositorioAlocacao.AdicionarEmLoteAsync(alocacoes, cancellationToken);

        // Assign generated Ids and dates from persisted entities back to the partial responses (preserve original order)
        for (int i = 0; i < persistidas.Count && i < respostasParciais.Count; i++)
        {
            respostasParciais[i].Id = persistidas[i].Id;
            respostasParciais[i].DataHoraInicio = persistidas[i].DataHoraInicio;
            respostasParciais[i].DataHoraFim = persistidas[i].DataHoraFim;
        }

        var resultadoFinal = respostasParciais.OrderBy(r => r.DataHoraInicio).ToList();

        // Debug output for test investigation: write sequence of chair numbers to a temp file
        try
        {
            var seq = string.Join(",", resultadoFinal.Select(r => r.NumeroCadeira));
            System.IO.File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "alloc_sequence.txt"), seq);
        }
        catch { }

        return resultadoFinal;
    }
}
