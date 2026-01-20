using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Dominio.Entidades;

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

            var cadeira = cadeiras[indexCadeira];
            indexCadeira = (indexCadeira + 1) % cadeiras.Count;

            alocacoes.Add(new Alocacao
            {
                CadeiraId = cadeira.Id,
                DataHoraInicio = inicio,
                DataHoraFim = fim
            });

            respostas.Add(new AlocacaoRespostaDto
            {
                Id = 0,
                CadeiraId = cadeira.Id,
                NumeroCadeira = cadeira.Numero,
                DataHoraInicio = inicio,
                DataHoraFim = fim
            });
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
