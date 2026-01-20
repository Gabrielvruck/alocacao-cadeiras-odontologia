using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Contexto.Configurations;

public class CadeiraConfiguration : IEntityTypeConfiguration<Cadeira>
{
    public void Configure(EntityTypeBuilder<Cadeira> builder)
    {
        builder.ToTable("cadeiras");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Numero)
            .HasColumnName("numero")
            .IsRequired();
        builder.HasIndex(c => c.Numero).IsUnique();
        builder.Property(c => c.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(200)
            .IsRequired();
        builder.HasMany(c => c.Alocacoes)
            .WithOne(a => a.Cadeira)
            .HasForeignKey(a => a.CadeiraId);
    }
}
