using HelpDesk.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HelpDesk.Tests.Infrastructure;

public class SecuenciaCodigoTicketTests
{
    [Fact]
    [Trait("RN", "RN-03")]
    public void Secuencia_CodigoTicket_IniciaEn1EIncrementaEn1()
    {
        var secuencia = Secuencia();

        Assert.Equal(typeof(int), secuencia.Type);
        Assert.Equal(1, secuencia.StartValue);
        Assert.Equal(1, secuencia.IncrementBy);
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public void Secuencia_CodigoTicket_TieneMaximo999999SinCiclo()
    {
        var secuencia = Secuencia();

        Assert.Equal(999999, secuencia.MaxValue);
        Assert.False(secuencia.IsCyclic);
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public void ConsultaSiguienteValor_Constante_NombraLaSecuenciaDelModelo()
    {
        var secuencia = Secuencia();
        var esquema = secuencia.Schema ?? "dbo";
        var consulta = GeneradorCodigoTicket.ConsultaSiguienteValor;

        Assert.Equal($"SELECT NEXT VALUE FOR {esquema}.{secuencia.Name} AS Value", consulta);
    }

    private static IReadOnlySequence Secuencia() =>
        ModeloEfCore.Diseno.FindSequence(GeneradorCodigoTicket.NombreSecuencia)!;
}
