using Aplicacao.Interfaces;
using Aplicacao.Modelos;
using FluentValidation;

namespace Aplicacao.Validacoes;

public class AlocacaoSolicitacaoDtoValidador : AbstractValidator<AlocacaoSolicitacaoDto>
{
    public AlocacaoSolicitacaoDtoValidador(IRepositorioCadeira repositorioCadeira)
    {
        RuleFor(dto => dto.DataHoraInicio)
            .NotEmpty();

        RuleFor(dto => dto.DataHoraFim)
            .NotEmpty()
            .GreaterThan(dto => dto.DataHoraInicio)
            .WithMessage("A data/hora de fim deve ser maior que a data/hora de início.");

        RuleFor(dto => dto)
            .MustAsync(async (dto, cancellation) =>
            {
                var cadeiras = await repositorioCadeira.ListarAsync(cancellation);
                return cadeiras.Count > 0;
            })
            .WithMessage("Nenhuma cadeira disponível para alocação.")
            .WithName("Cadeiras");
    }
}
