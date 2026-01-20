using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using FluentValidation;

namespace Aplicacao.Validacoes
{
    public class CadeiraAtualizacaoDtoValidador : AbstractValidator<CadeiraAtualizacaoDto>
    {
        public CadeiraAtualizacaoDtoValidador(IRepositorioCadeira repositorioCadeira)
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Numero)
                .GreaterThan(0)
                .WithMessage("O número deve ser maior que zero.");

            RuleFor(x => x)
                .MustAsync(async (dto, ct) =>
                {
                    var existente = await repositorioCadeira.ObterPorNumeroAsync(dto.Numero, ct);

                    // não existe => ok
                    if (existente is null) return true;

                    // existe mas é a própria cadeira que está sendo editada => ok
                    return existente.Id == dto.Id;
                })
                .WithMessage("Já existe uma cadeira cadastrada com esse número.");

            RuleFor(x => x.Descricao)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}
