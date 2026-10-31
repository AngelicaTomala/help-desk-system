using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Infrastructure.Persistencia;

namespace HelpDesk.Tests.Integracion;

/// <summary>
/// Solo lee los usuarios semilla: no necesita transacción.
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class UsuarioRepositorioIntegracionTests(BaseDePruebasFixture baseDePruebas)
{
    [Theory]
    [InlineData(1, "Ana Solicitante", Rol.Solicitante, true)]
    [InlineData(6, "Fabián Técnico", Rol.Tecnico, true)]
    [InlineData(9, "Irene Técnico", Rol.Tecnico, false)]
    [InlineData(10, "Jorge Supervisor", Rol.Supervisor, true)]
    public async Task ObtenerPorId_UsuarioExistente_DevuelveRolYActivoSinRastrear(int id, string nombre, Rol rol, bool activo)
    {
        await using var context = baseDePruebas.CrearContexto();

        var usuario = await new UsuarioRepositorio(context).ObtenerPorIdAsync(id, CancellationToken.None);

        Assert.NotNull(usuario);
        Assert.Equal((id, nombre, rol, activo), (usuario.Id, usuario.Nombre, usuario.Rol, usuario.Activo));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [InlineData(11)]
    [InlineData(int.MaxValue)]
    public async Task ObtenerPorId_UsuarioInexistente_DevuelveNull(int id)
    {
        await using var context = baseDePruebas.CrearContexto();

        var usuario = await new UsuarioRepositorio(context).ObtenerPorIdAsync(id, CancellationToken.None);

        Assert.Null(usuario);
    }
}
