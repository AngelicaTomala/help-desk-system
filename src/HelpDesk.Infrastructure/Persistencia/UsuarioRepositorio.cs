using HelpDesk.Application.Abstracciones;
using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistencia;

/// <summary>
/// Lectura de usuarios sin rastreo: los usuarios son datos semilla y el MVP no los modifica (S-01, decisión 97).
/// </summary>
public class UsuarioRepositorio(HelpDeskDbContext context) : IUsuarioRepositorio
{
    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken) =>
        context.Usuarios
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
}
