using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using Aplicacao.Servicos;
using Dominio.Entidades;
using FluentAssertions;
using Moq;
using Xunit;

namespace Aplicacao.Tests;

public class CadeiraServicoTests
{
    [Fact]
    public async Task CriarAsync_RetornaDto_QuandoCriado()
    {
        var repo = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        repo.Setup(r => r.AdicionarAsync(It.IsAny<Cadeira>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cadeira c, CancellationToken _) =>
            {
                c.Id = 123;
                return c;
            });

        var servico = new CadeiraServico(repo.Object);

        var dto = new CadeiraCriacaoDto { Numero = 5, Descricao = "Teste" };

        var resultado = await servico.CriarAsync(dto, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(123);
        resultado.Numero.Should().Be(dto.Numero);
        resultado.Descricao.Should().Be(dto.Descricao);

        repo.Verify(r => r.AdicionarAsync(It.Is<Cadeira>(c => c.Numero == dto.Numero && c.Descricao == dto.Descricao), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyAll();
    }

    [Fact]
    public async Task ListarAsync_RetornaTodos()
    {
        var repo = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        var cadeiras = new List<Cadeira>
        {
            new() { Id = 1, Numero = 1, Descricao = "A" },
            new() { Id = 2, Numero = 2, Descricao = "B" }
        };

        repo.Setup(r => r.ListarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(cadeiras);

        var servico = new CadeiraServico(repo.Object);

        var resultado = await servico.ListarAsync(CancellationToken.None);

        resultado.Should().HaveCount(2);
        resultado.Select(x => x.Id).Should().Equal(1,2);

        repo.Verify(r => r.ListarAsync(It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyAll();
    }

    [Fact]
    public async Task ObterPorIdAsync_RetornaDto_QuandoEncontrado()
    {
        var repo = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        var cadeira = new Cadeira { Id = 42, Numero = 10, Descricao = "X" };

        repo.Setup(r => r.ObterPorIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cadeira);

        var servico = new CadeiraServico(repo.Object);

        var resultado = await servico.ObterPorIdAsync(42, CancellationToken.None);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(42);
        resultado.Numero.Should().Be(10);

        repo.VerifyAll();
    }

    [Fact]
    public async Task ObterPorIdAsync_RetornaNull_QuandoNaoEncontrado()
    {
        var repo = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        repo.Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((Cadeira?)null);

        var servico = new CadeiraServico(repo.Object);

        var resultado = await servico.ObterPorIdAsync(99, CancellationToken.None);

        resultado.Should().BeNull();
        repo.VerifyAll();
    }

    [Fact]
    public async Task AtualizarAsync_RetornaFalse_QuandoNaoExiste()
    {
        var repo = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        repo.Setup(r => r.ObterPorIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((Cadeira?)null);

        var servico = new CadeiraServico(repo.Object);

        var dto = new Aplicacao.Modelos.CadeiraAtualizacaoDto { Id = 5, Numero = 7, Descricao = "N" };
        var resultado = await servico.AtualizarAsync(5, dto, CancellationToken.None);

        resultado.Should().BeFalse();
        repo.VerifyAll();
    }

    [Fact]
    public async Task AtualizarAsync_RetornaTrue_QuandExiste()
    {
        var repo = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        var cadeira = new Cadeira { Id = 5, Numero = 1, Descricao = "Old" };

        repo.Setup(r => r.ObterPorIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(cadeira);
        repo.Setup(r => r.AtualizarAsync(It.IsAny<Cadeira>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var servico = new CadeiraServico(repo.Object);

        var dto = new Aplicacao.Modelos.CadeiraAtualizacaoDto { Id = 5, Numero = 2, Descricao = "New" };
        var resultado = await servico.AtualizarAsync(5, dto, CancellationToken.None);

        resultado.Should().BeTrue();
        repo.Verify(r => r.AtualizarAsync(It.Is<Cadeira>(c => c.Id == 5 && c.Numero == dto.Numero && c.Descricao == dto.Descricao), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyAll();
    }

    [Fact]
    public async Task RemoverAsync_RetornaFalse_QuandoNaoExiste()
    {
        var repo = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        repo.Setup(r => r.ObterPorIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync((Cadeira?)null);

        var servico = new CadeiraServico(repo.Object);

        var resultado = await servico.RemoverAsync(7, CancellationToken.None);

        resultado.Should().BeFalse();
        repo.VerifyAll();
    }

    [Fact]
    public async Task RemoverAsync_RetornaTrue_QuandoExiste()
    {
        var repo = new Mock<IRepositorioCadeira>(MockBehavior.Strict);
        var cadeira = new Cadeira { Id = 7, Numero = 9, Descricao = "D" };

        repo.Setup(r => r.ObterPorIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(cadeira);
        repo.Setup(r => r.RemoverAsync(It.IsAny<Cadeira>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var servico = new CadeiraServico(repo.Object);

        var resultado = await servico.RemoverAsync(7, CancellationToken.None);

        resultado.Should().BeTrue();
        repo.Verify(r => r.RemoverAsync(It.Is<Cadeira>(c => c.Id == 7), It.IsAny<CancellationToken>()), Times.Once);
        repo.VerifyAll();
    }
}
