using HelpDesk.Domain.Entidades;
using HelpDesk.Infrastructure.Persistencia;

namespace HelpDesk.Tests.Infrastructure;

public class ConversorFechaUtcTests
{
    [Fact]
    public void ConvertFromProvider_FechaUnspecified_DevuelveKindUtc()
    {
        var leida = new DateTime(2026, 10, 28, 14, 30, 0, DateTimeKind.Unspecified);

        var fecha = (DateTime)new ConversorFechaUtc().ConvertFromProvider(leida)!;

        Assert.Equal(DateTimeKind.Utc, fecha.Kind);
        Assert.Equal(leida.Ticks, fecha.Ticks);
    }

    [Fact]
    public void ConvertToProvider_FechaUtc_GuardaSinCambios()
    {
        var ahora = new DateTime(2026, 10, 28, 14, 30, 0, DateTimeKind.Utc);

        var guardada = (DateTime)new ConversorFechaUtc().ConvertToProvider(ahora)!;

        Assert.Equal(ahora, guardada);
    }

    [Theory]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaCreacion))]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaActualizacion))]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaVencimiento))]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaResolucion))]
    [InlineData(typeof(Ticket), nameof(Ticket.FechaCierre))]
    [InlineData(typeof(Comentario), nameof(Comentario.FechaCreacion))]
    public void PropiedadFecha_EnModelo_UsaConversorFechaUtc(Type entidad, string propiedad)
    {
        var conversor = ModeloEfCore.Propiedad(entidad, propiedad).GetValueConverter();

        Assert.IsType<ConversorFechaUtc>(conversor);
    }
}
