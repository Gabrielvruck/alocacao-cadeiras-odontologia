using Aplicacao.Modelos;
using FluentValidation;

namespace Aplicacao.Validacoes;

public class AlocacaoSolicitacaoDtoValidador : AbstractValidator<AlocacaoSolicitacaoDto>
{
    public AlocacaoSolicitacaoDtoValidador()
    {
        RuleFor(dto => dto.DataHoraInicio)
            .NotEmpty();

        RuleFor(dto => dto.DataHoraFim)
            .NotEmpty()
            .GreaterThan(dto => dto.DataHoraInicio)
            .WithMessage("A data/hora de fim deve ser maior que a data/hora de início.");
    }
}
