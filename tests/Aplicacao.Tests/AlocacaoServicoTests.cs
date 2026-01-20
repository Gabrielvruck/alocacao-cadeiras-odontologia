using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Aplicacao.Servicos;
using Dominio.Entidades;
using FluentAssertions;
using Moq;
using Xunit;
namespace Aplicacao.Tests
{
    public class AlocacaoServicoTests
    {
        /// <summary>
        /// Verifica que a alocação automática distribui slots em rodízio entre cadeiras disponíveis.
        /// </summary>
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

            // quando consultado por alocações no período, não há alocações existentes
            repoCadeira
                .Setup(r => r.ObterAlocacoesPorCadeirasNoPeriodoAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

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

        /// <summary>
        /// Verifica que quando uma cadeira já possui alocação conflitante, ela é ignorada.
        /// </summary>
        [Fact]
        public async Task AlocarAutomaticamenteAsync_IgnoraCadeiraComConflito()
        {
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

            // simula que a cadeira Id=10 já está ocupada no primeiro slot
            repoCadeira
                .Setup(r => r.ObterAlocacoesPorCadeirasNoPeriodoAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, DateTime inicio, DateTime fim, CancellationToken ct) =>
                {
                    return ids.Contains(10) ? [new() { Id = 1, CadeiraId = 10, DataHoraInicio = inicio, DataHoraFim = fim }] : [];
                });

            repoAlocacao
                .Setup(r => r.AdicionarEmLoteAsync(It.IsAny<List<Alocacao>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<Alocacao> entrada, CancellationToken _) =>
                {
                    var id = 1;
                    foreach (var a in entrada)
                        a.Id = id++;
                    return entrada;
                });

            var servico = new AlocacaoServico(repoCadeira.Object, repoAlocacao.Object);

            var solicitacao = new AlocacaoSolicitacaoDto
            {
                DataHoraInicio = new DateTime(2024, 1, 1, 8, 0, 0),
                DataHoraFim = new DateTime(2024, 1, 1, 11, 0, 0)
            };

            var resultado = await servico.AlocarAutomaticamenteAsync(solicitacao, CancellationToken.None);

            // a cadeira 10 está ocupada em todos os slots no comportamento simulado,
            // portanto a alocação deve usar a cadeira 20 para todos os slots
            resultado.Should().HaveCount(3);
            resultado.Select(x => x.CadeiraId).Should().OnlyContain(id => id == 20);

            repoCadeira.VerifyAll();
            repoAlocacao.VerifyAll();
        }

        /// <summary>
        /// Verifica que quando todas as cadeiras estão ocupadas em um slot, uma exceção é lançada.
        /// </summary>
        [Fact]
        public async Task AlocarAutomaticamenteAsync_DisparaExcecaoQuandoSemDisponibilidade()
        {
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

            // simula que ambas cadeiras estão ocupadas em qualquer período
            repoCadeira
                .Setup(r => r.ObterAlocacoesPorCadeirasNoPeriodoAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<int> ids, DateTime inicio, DateTime fim, CancellationToken ct) =>
                {
                    return ids.Select(id => new Alocacao { Id = id, CadeiraId = id, DataHoraInicio = inicio, DataHoraFim = fim }).ToList();
                });

            var servico = new AlocacaoServico(repoCadeira.Object, repoAlocacao.Object);

            var solicitacao = new AlocacaoSolicitacaoDto
            {
                DataHoraInicio = new DateTime(2024, 1, 1, 8, 0, 0),
                DataHoraFim = new DateTime(2024, 1, 1, 9, 0, 0)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await servico.AlocarAutomaticamenteAsync(solicitacao, CancellationToken.None);
            });

            repoCadeira.VerifyAll();
        }
    }
}

