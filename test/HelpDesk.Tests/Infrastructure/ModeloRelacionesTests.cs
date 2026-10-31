using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HelpDesk.Tests.Infrastructure;

public class ModeloRelacionesTests
{
    [Theory]
    [InlineData(typeof(Ticket), nameof(Ticket.SolicitanteId))]
    [InlineData(typeof(Ticket), nameof(Ticket.TecnicoId))]
    [InlineData(typeof(Comentario), nameof(Comentario.AutorId))]
    public void ClaveForanea_HaciaUsuario_UsaRestrict(Type entidad, string propiedad)
    {
        var claveForanea = ClaveForanea(entidad, propiedad);

        Assert.Equal(typeof(Usuario), claveForanea.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, claveForanea.DeleteBehavior);
    }

    [Fact]
    public void ClaveForanea_ComentarioTicketId_UsaCascade()
    {
        var claveForanea = ClaveForanea(typeof(Comentario), nameof(Comentario.TicketId));

        Assert.Equal(typeof(Ticket), claveForanea.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, claveForanea.DeleteBehavior);
    }

    [Fact]
    public void ClaveForanea_TecnicoId_EsOpcional()
    {
        var claveForanea = ClaveForanea(typeof(Ticket), nameof(Ticket.TecnicoId));

        Assert.False(claveForanea.IsRequired);
    }

    [Fact]
    public void Comentarios_SeMapeaConCampoPrivado_UsaAccesoPorCampo()
    {
        var navegacion = ModeloEfCore.Diseno.FindEntityType(typeof(Ticket))!.FindNavigation(nameof(Ticket.Comentarios))!;

        Assert.Equal("_comentarios", navegacion.FieldInfo?.Name);
        Assert.Equal(PropertyAccessMode.Field, navegacion.GetPropertyAccessMode());
        Assert.Equal(nameof(Comentario.TicketId), navegacion.ForeignKey.Properties.Single().Name);
    }

    private static IForeignKey ClaveForanea(Type entidad, string propiedad) =>
        ModeloEfCore.Diseno.FindEntityType(entidad)!
            .GetForeignKeys()
            .Single(fk => fk.Properties.Count == 1 && fk.Properties[0].Name == propiedad);
}
