using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Infra.Contexto;

public class AplicacaoDbContext : DbContext
{
    public AplicacaoDbContext(DbContextOptions<AplicacaoDbContext> options) : base(options)
    {
    }

    public DbSet<Cadeira> Cadeiras => Set<Cadeira>();
    public DbSet<Alocacao> Alocacoes => Set<Alocacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cadeira>(entidade =>
        {
            entidade.ToTable("cadeiras");
            entidade.HasKey(cadeira => cadeira.Id);
            entidade.Property(cadeira => cadeira.Numero)
                .HasColumnName("numero")
                .IsRequired();
            entidade.Property(cadeira => cadeira.Descricao)
                .HasColumnName("descricao")
                .HasMaxLength(200)
                .IsRequired();
        });

        modelBuilder.Entity<Alocacao>(entidade =>
        {
            entidade.ToTable("alocacoes");
            entidade.HasKey(alocacao => alocacao.Id);
            entidade.Property(alocacao => alocacao.CadeiraId)
                .HasColumnName("cadeira_id")
                .IsRequired();
            entidade.Property(alocacao => alocacao.DataHoraInicio)
                .HasColumnName("data_hora_inicio")
                .IsRequired();
            entidade.Property(alocacao => alocacao.DataHoraFim)
                .HasColumnName("data_hora_fim")
                .IsRequired();
            entidade.HasOne(alocacao => alocacao.Cadeira)
                .WithMany(cadeira => cadeira.Alocacoes)
                .HasForeignKey(alocacao => alocacao.CadeiraId);
        });
    }
}
