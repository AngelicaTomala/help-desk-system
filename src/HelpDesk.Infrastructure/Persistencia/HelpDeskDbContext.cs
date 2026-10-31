using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistencia;

public class HelpDeskDbContext(DbContextOptions<HelpDeskDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<Comentario> Comentarios => Set<Comentario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HelpDeskDbContext).Assembly);
    }

    /// <summary>
    /// Todas las fechas se leen como UTC para serializarlas con <c>Z</c> (contrato §1.6, decisión 60).
    /// </summary>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<ConversorFechaUtc>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<ConversorFechaUtc>();
    }
}
