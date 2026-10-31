using HelpDesk.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Tests.Infrastructure;

public class ModeloColumnasTests
{
    [Theory]
    [InlineData(typeof(Ticket), nameof(Ticket.Estado))]
    [InlineData(typeof(Ticket), nameof(Ticket.Prioridad))]
    [InlineData(typeof(Ticket), nameof(Ticket.Categoria))]
    [InlineData(typeof(Usuario), nameof(Usuario.Rol))]
    [InlineData(typeof(Comentario), nameof(Comentario.Tipo))]
    public void ColumnaEnum_SeGuardaComoTexto_EsNvarchar20(Type entidad, string propiedad)
    {
        var columna = ModeloEfCore.Propiedad(entidad, propiedad);

        Assert.Equal(typeof(string), columna.GetProviderClrType());
        Assert.Equal("nvarchar(20)", columna.GetColumnType());
    }

    [Fact]
    [Trait("RN", "RN-01")]
    public void Titulo_Longitud_EsMaxima150()
    {
        var columna = ModeloEfCore.Propiedad(typeof(Ticket), nameof(Ticket.Titulo));

        Assert.Equal(150, columna.GetMaxLength());
        Assert.False(columna.IsNullable);
    }

    [Fact]
    [Trait("RN", "RN-01")]
    public void Descripcion_Longitud_EsMaxima2000()
    {
        var columna = ModeloEfCore.Propiedad(typeof(Ticket), nameof(Ticket.Descripcion));

        Assert.Equal(2000, columna.GetMaxLength());
        Assert.False(columna.IsNullable);
    }

    [Fact]
    [Trait("RN", "RN-09")]
    public void Texto_Longitud_EsMaxima2000()
    {
        var columna = ModeloEfCore.Propiedad(typeof(Comentario), nameof(Comentario.Texto));

        Assert.Equal(2000, columna.GetMaxLength());
        Assert.False(columna.IsNullable);
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public void Codigo_Longitud_Es9()
    {
        var columna = ModeloEfCore.Propiedad(typeof(Ticket), nameof(Ticket.Codigo));

        Assert.Equal(9, columna.GetMaxLength());
        Assert.False(columna.IsNullable);
    }

    [Theory]
    [InlineData(nameof(Usuario.Nombre), 100)]
    [InlineData(nameof(Usuario.Email), 150)]
    public void ColumnaUsuario_Longitud_CoincideConModelo(string propiedad, int longitud)
    {
        var columna = ModeloEfCore.Propiedad(typeof(Usuario), propiedad);

        Assert.Equal(longitud, columna.GetMaxLength());
        Assert.False(columna.IsNullable);
    }

    [Theory]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaCreacion))]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaActualizacion))]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaVencimiento))]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaResolucion))]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaCierre))]
    [InlineData(typeof(Comentario), nameof(Comentario.FechaCreacion))]
    public void ColumnaFecha_EnModelo_EsDatetime2(Type entidad, string propiedad)
    {
        var columna = ModeloEfCore.Propiedad(entidad, propiedad);

        Assert.Equal("datetime2", columna.GetColumnType());
    }
}
