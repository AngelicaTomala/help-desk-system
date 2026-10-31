using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistencia.Configuraciones;

public class UsuarioConfiguracion : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre).HasMaxLength(100);
        builder.Property(u => u.Email).HasMaxLength(150);
        builder.Property(u => u.Rol).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique();

        // Usuarios semilla de 01-modelo-dominio.md §8 con los Id 1 a 10 (S-01, decisión 89).
        builder.HasData(
            new { Id = 1, Nombre = "Ana Solicitante", Email = "ana.solicitante@example.com", Rol = Rol.Solicitante, Activo = true },
            new { Id = 2, Nombre = "Bruno Solicitante", Email = "bruno.solicitante@example.com", Rol = Rol.Solicitante, Activo = true },
            new { Id = 3, Nombre = "Carla Solicitante", Email = "carla.solicitante@example.com", Rol = Rol.Solicitante, Activo = true },
            new { Id = 4, Nombre = "Diego Solicitante", Email = "diego.solicitante@example.com", Rol = Rol.Solicitante, Activo = true },
            new { Id = 5, Nombre = "Elena Solicitante", Email = "elena.solicitante@example.com", Rol = Rol.Solicitante, Activo = true },
            new { Id = 6, Nombre = "Fabián Técnico", Email = "fabian.tecnico@example.com", Rol = Rol.Tecnico, Activo = true },
            new { Id = 7, Nombre = "Gabriela Técnico", Email = "gabriela.tecnico@example.com", Rol = Rol.Tecnico, Activo = true },
            new { Id = 8, Nombre = "Hugo Técnico", Email = "hugo.tecnico@example.com", Rol = Rol.Tecnico, Activo = true },
            new { Id = 9, Nombre = "Irene Técnico", Email = "irene.tecnico@example.com", Rol = Rol.Tecnico, Activo = false },
            new { Id = 10, Nombre = "Jorge Supervisor", Email = "jorge.supervisor@example.com", Rol = Rol.Supervisor, Activo = true });
    }
}
