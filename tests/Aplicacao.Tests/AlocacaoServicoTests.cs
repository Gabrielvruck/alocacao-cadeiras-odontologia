using Aplicacao.Modelos;
using Aplicacao.Servicos;
using FluentAssertions;
using Infra.Contexto;
using Infra.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Aplicacao.Tests;

public class AlocacaoServicoTests
{
    private static AplicacaoDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AplicacaoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AplicacaoDbContext(options);
    }

    [Fact]
    public async Task AlocarAutomaticamenteAsync_DistribuiPorRodizio()
    {
        using var contexto = CriarContexto();
        contexto.Cadeiras.AddRange(
            new Dominio.Entidades.Cadeira { Numero = 1, Descricao = "Cadeira 1" },
            new Dominio.Entidades.Cadeira { Numero = 2, Descricao = "Cadeira 2" }
        );
        await contexto.SaveChangesAsync();

        var repositorioCadeira = new RepositorioCadeira(contexto);
        var repositorioAlocacao = new RepositorioAlocacao(contexto);
        var servico = new AlocacaoServico(repositorioCadeira, repositorioAlocacao);

        var solicitacao = new AlocacaoSolicitacaoDto
        {
            DataHoraInicio = new DateTime(2024, 1, 1, 8, 0, 0),
            DataHoraFim = new DateTime(2024, 1, 1, 11, 0, 0)
        };

        var resultado = await servico.AlocarAutomaticamenteAsync(solicitacao, CancellationToken.None);

        resultado.Should().HaveCount(3);
        resultado.Select(alocacao => alocacao.NumeroCadeira)
            .Should().ContainInOrder(new[] { 1, 2, 1 });
    }

    [Fact]
    public async Task AlocarAutomaticamenteAsync_RetornaListaVazia_QuandoNaoHaCadeiras()
    {
        using var contexto = CriarContexto();
        var repositorioCadeira = new RepositorioCadeira(contexto);
        var repositorioAlocacao = new RepositorioAlocacao(contexto);
        var servico = new AlocacaoServico(repositorioCadeira, repositorioAlocacao);

        var solicitacao = new AlocacaoSolicitacaoDto
        {
            DataHoraInicio = new DateTime(2024, 1, 1, 8, 0, 0),
            DataHoraFim = new DateTime(2024, 1, 1, 9, 0, 0)
        };

        var resultado = await servico.AlocarAutomaticamenteAsync(solicitacao, CancellationToken.None);

        resultado.Should().BeEmpty();
    }
}
