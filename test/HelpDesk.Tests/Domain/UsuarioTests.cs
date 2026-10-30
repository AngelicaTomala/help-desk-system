using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Tests.Domain;

public class UsuarioTests
{
    [Theory]
    [Trait("RN", "RN-06")]
    [InlineData(Rol.Tecnico, true, true)]
    [InlineData(Rol.Tecnico, false, false)]
    [InlineData(Rol.Solicitante, true, false)]
    [InlineData(Rol.Supervisor, true, false)]
    public void EsTecnicoActivo_SegunRolYActivo_DevuelveSoloTecnicoActivo(Rol rol, bool activo, bool esperado)
    {
        var usuario = new Usuario("Fabián Técnico", "fabian.tecnico@example.com", rol, activo);

        Assert.Equal(esperado, usuario.EsTecnicoActivo());
    }
}
