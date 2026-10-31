using HelpDesk.Domain;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;

namespace HelpDesk.Tests.Domain;

public class PoliticaSlaTests
{
    [Theory]
    [Trait("RN", "RN-10")]
    [Trait("CA", "CA-05.1")]
    [InlineData(Prioridad.Critica, 2)]
    [InlineData(Prioridad.Alta, 4)]
    [InlineData(Prioridad.Media, 8)]
    [InlineData(Prioridad.Baja, 24)]
    public void HorasPara_PrioridadDefinida_DevuelveHorasDeRn10(Prioridad prioridad, int horas)
    {
        Assert.Equal(horas, PoliticaSla.HorasPara(prioridad));
    }

    [Fact]
    [Trait("RN", "RN-02")]
    [Trait("RN", "RN-10")]
    public void HorasPara_PrioridadNoDefinida_LanzaInvarianteViolada()
    {
        var ex = Assert.Throws<InvarianteVioladaException>(() => PoliticaSla.HorasPara((Prioridad)7));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
    }
}
