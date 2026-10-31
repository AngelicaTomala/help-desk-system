using HelpDesk.Application.Abstracciones;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Tests.Application.Dobles;

internal sealed class UsuarioActualFalso(int id, Rol rol) : IUsuarioActual
{
    public int Id { get; } = id;

    public Rol Rol { get; } = rol;
}
