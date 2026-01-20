using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Dominio.Entidades;
using System.Linq;

namespace Aplicacao.Servicos;

public class AlocacaoServico(IRepositorioCadeira repositorioCadeira, IRepositorioAlocacao repositorioAlocacao)
{
    private static readonly TimeSpan Slot = TimeSpan.FromHours(1);

    private readonly IRepositorioCadeira _repositorioCadeira = repositorioCadeira;
    private readonly IRepositorioAlocacao _repositorioAlocacao = repositorioAlocacao;
 
    /// <summary>
    /// Gera alocações automaticamente no intervalo informado, distribuindo de forma round-robin entre cadeiras.
    /// </summary>
    /// <param name="solicitacao">DTO com data/hora de início e fim.</param>
    /// <param name="cancellationToken">Token que permite cancelar a operação.</param>
    /// <returns>Lista de DTOs de alocação criadas e persistidas.</returns>
    public async Task<List<AlocacaoRespostaDto>> AlocarAutomaticamenteAsync(
        AlocacaoSolicitacaoDto solicitacao,
        CancellationToken cancellationToken)
    {
        // valida intervalo: fim deve ser posterior ao início
        if (solicitacao.DataHoraFim <= solicitacao.DataHoraInicio)
            return [];

        var cadeiras = (await _repositorioCadeira.ListarAsync(cancellationToken))
            .OrderBy(c => c.Numero)
            .ToList();

        // se não há cadeiras cadastradas, retorna lista vazia
        if (cadeiras.Count == 0)
            return [];

        var alocacoes = new List<Alocacao>();
        var respostas = new List<AlocacaoRespostaDto>();

        var indexCadeira = 0;

        // divide o intervalo em slots de 1 hora (constante Slot) e aloca em round-robin
        for (var inicio = solicitacao.DataHoraInicio; inicio < solicitacao.DataHoraFim; inicio = inicio.Add(Slot))
        {
            var fim = inicio.Add(Slot);
            if (fim > solicitacao.DataHoraFim)
                fim = solicitacao.DataHoraFim;

            // otimização: carrega de uma única vez as alocações no período para todas as cadeiras
            var cadeiraIds = cadeiras.Select(c => c.Id).ToList();
            var alocsNoPeriodo = await _repositorioCadeira.ObterAlocacoesPorCadeirasNoPeriodoAsync(cadeiraIds, inicio, fim, cancellationToken);

            // tenta encontrar uma cadeira sem conflito para este slot, começando em indexCadeira
            Alocacao? criada = null;
            AlocacaoRespostaDto? resposta = null;

            for (var attempt = 0; attempt < cadeiras.Count; attempt++)
            {
                var candIndex = (indexCadeira + attempt) % cadeiras.Count;
                var cadeiraCand = cadeiras[candIndex];

                var existeConflito = alocsNoPeriodo.Any(a => a.CadeiraId == cadeiraCand.Id);

                if (existeConflito)
                    continue;

                // sem conflito: cria alocação para este slot
                criada = new Alocacao
                {
                    CadeiraId = cadeiraCand.Id,
                    DataHoraInicio = inicio,
                    DataHoraFim = fim
                };

                resposta = new AlocacaoRespostaDto
                {
                    Id = 0,
                    CadeiraId = cadeiraCand.Id,
                    NumeroCadeira = cadeiraCand.Numero,
                    DataHoraInicio = inicio,
                    DataHoraFim = fim
                };

                // avança o ponteiro principal para preservar round-robin
                indexCadeira = (candIndex + 1) % cadeiras.Count;
                break;
            }

            // se encontrou cadeira disponível, adiciona à lista de persistência
            if (criada is not null && resposta is not null)
            {
                alocacoes.Add(criada);
                respostas.Add(resposta);
            }
            else
            {
                // se nenhuma cadeira disponível neste slot, lançar exceção para indicar indisponibilidade
                throw new InvalidOperationException($"Nenhuma cadeira disponível para o intervalo {inicio:o} - {fim:o}.");
            }
        }

        // persiste em lote e recupera registros com relacionamentos carregados
        var persistidas = await _repositorioAlocacao.AdicionarEmLoteAsync(alocacoes, cancellationToken);

        // atualiza os ids e datas a partir das entidades persistidas
        for (var i = 0; i < respostas.Count && i < persistidas.Count; i++)
        {
            respostas[i].Id = persistidas[i].Id;
            respostas[i].DataHoraInicio = persistidas[i].DataHoraInicio;
            respostas[i].DataHoraFim = persistidas[i].DataHoraFim;
        }

        return respostas;
    }

    public async Task<PaginacaoRespostaDto<AlocacaoRespostaDto>> ListarPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var (items, total) = await _repositorioAlocacao.ListarPaginadoAsync(pageNumber, pageSize, cancellationToken);

        var dtos = items.Select(a => new AlocacaoRespostaDto
        {
            Id = a.Id,
            CadeiraId = a.CadeiraId,
            NumeroCadeira = a.Cadeira?.Numero ?? 0,
            DataHoraInicio = a.DataHoraInicio,
            DataHoraFim = a.DataHoraFim
        }).ToList();

        return new PaginacaoRespostaDto<AlocacaoRespostaDto>
        {
            Items = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }
}
