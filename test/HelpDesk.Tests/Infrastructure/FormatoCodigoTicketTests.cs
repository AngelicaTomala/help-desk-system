using HelpDesk.Infrastructure.Persistencia;

namespace HelpDesk.Tests.Infrastructure;

public class FormatoCodigoTicketTests
{
    [Theory]
    [Trait("RN", "RN-03")]
    [InlineData(1, "HD-000001")]
    [InlineData(9, "HD-000009")]
    [InlineData(10, "HD-000010")]
    [InlineData(999, "HD-000999")]
    [InlineData(1000, "HD-001000")]
    [InlineData(123456, "HD-123456")]
    [InlineData(999999, "HD-999999")]
    public void Formatear_NumeroEnRango_DevuelveCodigo(int numero, string esperado)
    {
        Assert.Equal(esperado, FormatoCodigoTicket.Formatear(numero));
    }

    [Theory]
    [Trait("RN", "RN-03")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1000000)]
    public void Formatear_NumeroFueraDeRango_LanzaArgumentOutOfRange(int numero)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FormatoCodigoTicket.Formatear(numero));
    }
}
