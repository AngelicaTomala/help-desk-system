using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Application.Abstracciones;

/// <summary>
/// Usuario de <c>X-Usuario-Id</c>, con alcance por petición; lo fija el middleware de la Api (decisión 97).
/// Leerlo sin un usuario fijado es un error de programación: la implementación lanza <see cref="InvalidOperationException"/>.
/// </summary>
public interface IUsuarioActual
{
    int Id { get; }

    Rol Rol { get; }
}
