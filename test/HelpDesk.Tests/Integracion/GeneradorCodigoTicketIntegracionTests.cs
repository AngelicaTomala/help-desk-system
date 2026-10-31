using HelpDesk.Infrastructure.Persistencia;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Integracion;

/// <summary>
/// La SEQUENCE no es transaccional: cada prueba la reinicia antes de usarla, para no depender del orden.
/// </summary>
[Collection(BaseDePruebasColeccion.Nombre)]
public class GeneradorCodigoTicketIntegracionTests(BaseDePruebasFixture baseDePruebas)
{
    // SQL constante, sin concatenación ni interpolación (RNF-02). El valor 1 es explícito porque
    // RESTART WITH también cambia el valor inicial: un RESTART sin WITH volvería a 999999.
    private const string ReiniciarSecuencia = "ALTER SEQUENCE dbo.CodigoTicket RESTART WITH 1";
    private const string ReiniciarSecuenciaEnMaximo = "ALTER SEQUENCE dbo.CodigoTicket RESTART WITH 999999";

    // "The sequence object has reached its minimum or maximum value."
    private const int ErrorSecuenciaAgotada = 11728;

    [Fact]
    [Trait("RN", "RN-03")]
    public async Task GenerarAsync_SecuenciaEnValorInicial_DevuelveHD000001()
    {
        await using var context = baseDePruebas.CrearContexto();
        await context.Database.ExecuteSqlRawAsync(ReiniciarSecuencia);

        var codigo = await new GeneradorCodigoTicket(context).GenerarAsync(CancellationToken.None);

        Assert.Equal("HD-000001", codigo);
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public async Task GenerarAsync_VariasLlamadas_DevuelveCodigosUnicosYCrecientes()
    {
        await using var context = baseDePruebas.CrearContexto();
        await context.Database.ExecuteSqlRawAsync(ReiniciarSecuencia);
        var generador = new GeneradorCodigoTicket(context);

        var codigos = new List<string>();
        for (var i = 0; i < 5; i++)
        {
            codigos.Add(await generador.GenerarAsync(CancellationToken.None));
        }

        Assert.Equal(["HD-000001", "HD-000002", "HD-000003", "HD-000004", "HD-000005"], codigos);
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public async Task GenerarAsync_SecuenciaEnMaximo_DevuelveHD999999YLuegoFalla()
    {
        await using var context = baseDePruebas.CrearContexto();
        await context.Database.ExecuteSqlRawAsync(ReiniciarSecuenciaEnMaximo);
        try
        {
            var generador = new GeneradorCodigoTicket(context);

            Assert.Equal("HD-999999", await generador.GenerarAsync(CancellationToken.None));

            var excepcion = await Assert.ThrowsAsync<SqlException>(() => generador.GenerarAsync(CancellationToken.None));
            Assert.Equal(ErrorSecuenciaAgotada, excepcion.Number);
        }
        finally
        {
            await context.Database.ExecuteSqlRawAsync(ReiniciarSecuencia);
        }
    }
}
