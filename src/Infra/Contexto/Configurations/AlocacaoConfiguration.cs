using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Contexto.Configurations;

public class AlocacaoConfiguration : IEntityTypeConfiguration<Alocacao>
{
    public void Configure(EntityTypeBuilder<Alocacao> builder)
    {
        builder.ToTable("alocacoes");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.CadeiraId)
            .HasColumnName("cadeira_id")
            .IsRequired();
        builder.Property(a => a.DataHoraInicio)
            .HasColumnName("data_hora_inicio")
            .IsRequired();
        builder.Property(a => a.DataHoraFim)
            .HasColumnName("data_hora_fim")
            .IsRequired();
        builder.HasOne(a => a.Cadeira)
            .WithMany(c => c.Alocacoes)
            .HasForeignKey(a => a.CadeiraId);
    }
}
