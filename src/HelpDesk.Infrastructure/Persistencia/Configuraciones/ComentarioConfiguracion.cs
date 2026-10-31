using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistencia.Configuraciones;

public class ComentarioConfiguracion : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Texto).HasMaxLength(Comentario.TextoLongitudMaxima);
        builder.Property(c => c.Tipo).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.AutorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
