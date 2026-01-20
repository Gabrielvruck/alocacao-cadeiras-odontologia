using Aplicacao.Modelos;
using FluentValidation;
using Aplicacao.Interfaces;

namespace Aplicacao.Validacoes;

public class CadeiraCriacaoDtoValidador : AbstractValidator<CadeiraCriacaoDto>
{
    public CadeiraCriacaoDtoValidador(IRepositorioCadeira repositorioCadeira)
    {
        RuleFor(dto => dto.Numero)
            .GreaterThan(0)
            .WithMessage("O número deve ser maior que zero.");

        RuleFor(dto => dto.Numero)
            .MustAsync(async (numero, cancellation) =>
            {
                var todas = await repositorioCadeira.ListarAsync(cancellation);
                return !todas.Any(c => c.Numero == numero);
            })
            .WithMessage("Já existe uma cadeira cadastrada com esse número.");

        RuleFor(dto => dto.Descricao)
            .NotEmpty()
            .MaximumLength(200);
    }
}
