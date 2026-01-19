using Aplicacao.Modelos;
using FluentValidation;

namespace Aplicacao.Validacoes;

public class CadeiraCriacaoDtoValidador : AbstractValidator<CadeiraCriacaoDto>
{
    public CadeiraCriacaoDtoValidador()
    {
        RuleFor(dto => dto.Numero)
            .GreaterThan(0)
            .WithMessage("O número deve ser maior que zero.");

        RuleFor(dto => dto.Descricao)
            .NotEmpty()
            .MaximumLength(200);
    }
}
