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
        // Apply entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AplicacaoDbContext).Assembly);
    }
}
