using HelpDesk.Application.Abstracciones;

namespace HelpDesk.Infrastructure.Persistencia;

/// <summary>
/// Guarda los cambios rastreados por el <see cref="HelpDeskDbContext"/> de la petición (decisión 97, RNF-04).
/// </summary>
public class UnidadDeTrabajo(HelpDeskDbContext context) : IUnidadDeTrabajo
{
    public Task GuardarCambiosAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
