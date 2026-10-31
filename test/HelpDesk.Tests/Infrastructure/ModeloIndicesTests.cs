using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HelpDesk.Tests.Infrastructure;

public class ModeloIndicesTests
{
    [Theory]
    [InlineData(nameof(Ticket.FechaCreacion))]
    [InlineData(nameof(Ticket.Estado))]
    [InlineData(nameof(Ticket.TecnicoId))]
    [InlineData(nameof(Ticket.SolicitanteId))]
    public void IndiceTicket_ColumnaDeRnf06_Existe(string propiedad)
    {
        Assert.NotNull(Indice(typeof(Ticket), propiedad));
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public void IndiceCodigo_Ticket_EsUnico()
    {
        var indice = Indice(typeof(Ticket), nameof(Ticket.Codigo));

        Assert.NotNull(indice);
        Assert.True(indice.IsUnique);
    }

    [Fact]
    public void IndiceEmail_Usuario_EsUnico()
    {
        var indice = Indice(typeof(Usuario), nameof(Usuario.Email));

        Assert.NotNull(indice);
        Assert.True(indice.IsUnique);
    }

    private static IIndex? Indice(Type entidad, string propiedad) =>
        ModeloEfCore.Diseno.FindEntityType(entidad)!
            .GetIndexes()
            .SingleOrDefault(i => i.Properties.Count == 1 && i.Properties[0].Name == propiedad);
}
