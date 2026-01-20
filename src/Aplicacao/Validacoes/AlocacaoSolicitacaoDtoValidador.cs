using Aplicacao.Modelos;
using FluentValidation;
using Aplicacao.Interfaces;
using Dominio.Entidades;

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
            .Must(dto => (dto.DataHoraFim - dto.DataHoraInicio).TotalHours >= 24)
            .WithMessage("A alocação deve ter no mínimo 1 dia.")
            .WithName("Periodo")
            .When(dto => dto.DataHoraFim > dto.DataHoraInicio);

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
