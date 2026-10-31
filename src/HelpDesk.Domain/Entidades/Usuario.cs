using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Domain.Entidades;

/// <summary>
/// Usuario cargado como dato semilla (S-01); no hay endpoint de registro.
/// </summary>
public class Usuario
{
    public Usuario(string nombre, string email, Rol rol, bool activo)
    {
        Nombre = nombre;
        Email = email;
        Rol = rol;
        Activo = activo;
    }

    private Usuario()
    {
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public Rol Rol { get; private set; }

    public bool Activo { get; private set; }

    /// <summary>
    /// Solo los técnicos activos se pueden asignar (RN-06).
    /// </summary>
    public bool EsTecnicoActivo() => Rol == Rol.Tecnico && Activo;
}
