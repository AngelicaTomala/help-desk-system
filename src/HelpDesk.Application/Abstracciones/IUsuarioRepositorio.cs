using HelpDesk.Domain.Entidades;

namespace HelpDesk.Application.Abstracciones;

/// <summary>
/// Lectura de usuarios (decisión 97). La usa el middleware de <c>X-Usuario-Id</c> a través de Application para
/// responder <c>401</c> (decisión 28); devuelve la entidad para reutilizar <see cref="Usuario.EsTecnicoActivo"/> (RN-06).
/// </summary>
public interface IUsuarioRepositorio
{
    /// <summary>
    /// Devuelve el usuario sin rastrear cambios; <c>null</c> si no existe.
    /// </summary>
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken);
}
