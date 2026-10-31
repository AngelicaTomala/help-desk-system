using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistencia.Configuraciones;

public class TicketConfiguracion : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Codigo).HasMaxLength(9);
        builder.Property(t => t.Titulo).HasMaxLength(Ticket.TituloLongitudMaxima);
        builder.Property(t => t.Descripcion).HasMaxLength(Ticket.DescripcionLongitudMaxima);
        builder.Property(t => t.Categoria).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Prioridad).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Estado).HasConversion<string>().HasMaxLength(20);

        // El MVP no borra usuarios: Restrict evita varios caminos de cascada hasta Comentario (decisión 87).
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(t => t.SolicitanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(t => t.TecnicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Comentarios)
            .WithOne()
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Comentarios)
            .HasField("_comentarios")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Índices de RNF-06 y código único (RN-03).
        builder.HasIndex(t => t.Codigo).IsUnique();
        builder.HasIndex(t => t.FechaCreacion);
        builder.HasIndex(t => t.Estado);
        builder.HasIndex(t => t.TecnicoId);
        builder.HasIndex(t => t.SolicitanteId);
    }
}
