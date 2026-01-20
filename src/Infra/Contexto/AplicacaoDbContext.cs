using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infra.Contexto;

public class AplicacaoDbContext(DbContextOptions<AplicacaoDbContext> options) : DbContext(options)
{
    public DbSet<Cadeira> Cadeiras => Set<Cadeira>();
    public DbSet<Alocacao> Alocacoes => Set<Alocacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AplicacaoDbContext).Assembly);
    }
}
