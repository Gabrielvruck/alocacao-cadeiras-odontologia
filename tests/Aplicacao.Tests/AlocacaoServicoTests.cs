using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Aplicacao.Servicos;
using Dominio.Entidades;
using FluentAssertions;
using Moq;
using Xunit;

public class AlocacaoServicoTests
{
    [Fact]
    public async Task AlocarAutomaticamenteAsync_DistribuiPorRodizio()
    {
        // Arrange
        var repoCadeira = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        var repoAlocacao = new Mock<IRepositorioAlocacao>(MockBehavior.Strict);

        var cadeiras = new List<Cadeira>
        {
            new() { Id = 10, Numero = 1, Descricao = "Cadeira 1" },
            new() { Id = 20, Numero = 2, Descricao = "Cadeira 2" },
        };

        repoCadeira
            .Setup(r => r.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cadeiras);

        repoAlocacao
            .Setup(r => r.AdicionarEmLoteAsync(It.IsAny<List<Alocacao>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Alocacao> entrada, CancellationToken _) =>
            {
                // Simula "SaveChanges" gerando Ids
                var id = 1;
                foreach (var a in entrada)
                    a.Id = id++;

                // Retorna exatamente na mesma ordem recebida
                return entrada;
            });

        var servico = new AlocacaoServico(repoCadeira.Object, repoAlocacao.Object);

        var solicitacao = new AlocacaoSolicitacaoDto
        {
            DataHoraInicio = new DateTime(2024, 1, 1, 8, 0, 0),
            DataHoraFim = new DateTime(2024, 1, 1, 11, 0, 0)
        };

        // Act
        var resultado = await servico.AlocarAutomaticamenteAsync(solicitacao, CancellationToken.None);

        // Assert
        resultado.Should().HaveCount(3);

        resultado.Select(x => x.NumeroCadeira)
            .Should().Equal(1, 2, 1);

        resultado.Select(x => x.CadeiraId)
            .Should().Equal(10, 20, 10);

        resultado.Select(x => (x.DataHoraInicio, x.DataHoraFim))
            .Should().Equal(
                (new DateTime(2024, 1, 1, 8, 0, 0), new DateTime(2024, 1, 1, 9, 0, 0)),
                (new DateTime(2024, 1, 1, 9, 0, 0), new DateTime(2024, 1, 1, 10, 0, 0)),
                (new DateTime(2024, 1, 1, 10, 0, 0), new DateTime(2024, 1, 1, 11, 0, 0))
            );

        // Verifica que o serviço chamou o repo de persistência com 3 itens
        repoAlocacao.Verify(r =>
            r.AdicionarEmLoteAsync(It.Is<List<Alocacao>>(l => l.Count == 3), It.IsAny<CancellationToken>()),
            Times.Once);

        repoCadeira.VerifyAll();
        repoAlocacao.VerifyAll();
    }
}
