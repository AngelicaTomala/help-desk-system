using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;

namespace HelpDesk.Tests.Infrastructure;

public class DatosSemillaTests
{
    [Fact]
    public void DatosSemilla_Usuarios_SonDiezConIds1a10()
    {
        var ids = Semilla().Select(u => (int)u[nameof(Usuario.Id)]!).Order();

        Assert.Equal(Enumerable.Range(1, 10), ids);
    }

    [Theory]
    [InlineData(1, "Ana Solicitante", "ana.solicitante@example.com", Rol.Solicitante, true)]
    [InlineData(2, "Bruno Solicitante", "bruno.solicitante@example.com", Rol.Solicitante, true)]
    [InlineData(3, "Carla Solicitante", "carla.solicitante@example.com", Rol.Solicitante, true)]
    [InlineData(4, "Diego Solicitante", "diego.solicitante@example.com", Rol.Solicitante, true)]
    [InlineData(5, "Elena Solicitante", "elena.solicitante@example.com", Rol.Solicitante, true)]
    [InlineData(6, "Fabián Técnico", "fabian.tecnico@example.com", Rol.Tecnico, true)]
    [InlineData(7, "Gabriela Técnico", "gabriela.tecnico@example.com", Rol.Tecnico, true)]
    [InlineData(8, "Hugo Técnico", "hugo.tecnico@example.com", Rol.Tecnico, true)]
    [InlineData(9, "Irene Técnico", "irene.tecnico@example.com", Rol.Tecnico, false)]
    [InlineData(10, "Jorge Supervisor", "jorge.supervisor@example.com", Rol.Supervisor, true)]
    public void DatosSemilla_Usuario_CoincideConTablaDelModelo(int id, string nombre, string email, Rol rol, bool activo)
    {
        var usuario = Semilla().Single(u => (int)u[nameof(Usuario.Id)]! == id);

        Assert.Equal(nombre, usuario[nameof(Usuario.Nombre)]);
        Assert.Equal(email, usuario[nameof(Usuario.Email)]);
        Assert.Equal(rol, usuario[nameof(Usuario.Rol)]);
        Assert.Equal(activo, usuario[nameof(Usuario.Activo)]);
    }

    private static IEnumerable<IDictionary<string, object?>> Semilla() =>
        ModeloEfCore.Diseno.FindEntityType(typeof(Usuario))!.GetSeedData();
}
