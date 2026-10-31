using HelpDesk.Application.Abstracciones;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Api.Identidad;

/// <summary>
/// Usuario de <c>X-Usuario-Id</c> para la petición en curso (decisión 97). El middleware lo fija una vez; leerlo sin
/// fijar es un error de programación.
/// </summary>
public sealed class UsuarioActual : IUsuarioActual
{
    private (int Id, Rol Rol)? _usuario;

    public int Id => Usuario.Id;

    public Rol Rol => Usuario.Rol;

    public void Fijar(int id, Rol rol)
    {
        if (_usuario is not null)
        {
            throw new InvalidOperationException("El usuario actual ya está fijado para esta petición.");
        }

        _usuario = (id, rol);
    }

    private (int Id, Rol Rol) Usuario =>
        _usuario ?? throw new InvalidOperationException("El usuario actual no está fijado para esta petición.");
}
