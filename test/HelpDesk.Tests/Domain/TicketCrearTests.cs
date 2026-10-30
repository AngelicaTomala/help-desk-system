using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;
using static HelpDesk.Tests.Domain.TicketsDePrueba;

namespace HelpDesk.Tests.Domain;

public class TicketCrearTests
{
    [Fact]
    [Trait("RN", "RN-03")]
    public void Crear_DatosValidos_QuedaAbiertoSinTecnicoYConFechasIgualesAAhora()
    {
        var ticket = Ticket.Crear(Codigo, Titulo, Descripcion, Categoria.Red, Prioridad.Alta, SolicitanteId, Ahora);

        Assert.Equal(EstadoTicket.Abierto, ticket.Estado);
        Assert.Null(ticket.TecnicoId);
        Assert.Equal(Codigo, ticket.Codigo);
        Assert.Equal(Titulo, ticket.Titulo);
        Assert.Equal(Descripcion, ticket.Descripcion);
        Assert.Equal(Categoria.Red, ticket.Categoria);
        Assert.Equal(Prioridad.Alta, ticket.Prioridad);
        Assert.Equal(SolicitanteId, ticket.SolicitanteId);
        Assert.Equal(Ahora, ticket.FechaCreacion);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
        Assert.Null(ticket.FechaResolucion);
        Assert.Null(ticket.FechaCierre);
        Assert.Empty(ticket.Comentarios);
    }

    [Theory]
    [Trait("RN", "RN-10")]
    [Trait("CA", "CA-01.2")]
    [Trait("CA", "CA-05.1")]
    [InlineData(Prioridad.Critica, 2)]
    [InlineData(Prioridad.Alta, 4)]
    [InlineData(Prioridad.Media, 8)]
    [InlineData(Prioridad.Baja, 24)]
    public void Crear_ConPrioridad_VencimientoEsCreacionMasHorasSla(Prioridad prioridad, int horas)
    {
        var ticket = Ticket.Crear(Codigo, Titulo, Descripcion, Categoria.Software, prioridad, SolicitanteId, Ahora);

        Assert.Equal(Ahora.AddHours(horas), ticket.FechaVencimiento);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [InlineData(Ticket.TituloLongitudMinima)]
    [InlineData(Ticket.TituloLongitudMaxima)]
    public void Crear_TituloEnLimite_SeAcepta(int longitud)
    {
        var titulo = Texto(longitud);

        var ticket = Ticket.Crear(Codigo, titulo, Descripcion, Categoria.Software, Prioridad.Media, SolicitanteId, Ahora);

        Assert.Equal(titulo, ticket.Titulo);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [InlineData(Ticket.TituloLongitudMinima - 1)]
    [InlineData(Ticket.TituloLongitudMaxima + 1)]
    public void Crear_TituloFueraDeLimite_LanzaInvarianteViolada(int longitud)
    {
        var ex = Assert.Throws<InvarianteVioladaException>(() =>
            Ticket.Crear(Codigo, Texto(longitud), Descripcion, Categoria.Software, Prioridad.Media, SolicitanteId, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [InlineData(Ticket.DescripcionLongitudMinima)]
    [InlineData(Ticket.DescripcionLongitudMaxima)]
    public void Crear_DescripcionEnLimite_SeAcepta(int longitud)
    {
        var descripcion = Texto(longitud);

        var ticket = Ticket.Crear(Codigo, Titulo, descripcion, Categoria.Software, Prioridad.Media, SolicitanteId, Ahora);

        Assert.Equal(descripcion, ticket.Descripcion);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [InlineData(Ticket.DescripcionLongitudMinima - 1)]
    [InlineData(Ticket.DescripcionLongitudMaxima + 1)]
    public void Crear_DescripcionFueraDeLimite_LanzaInvarianteViolada(int longitud)
    {
        var ex = Assert.Throws<InvarianteVioladaException>(() =>
            Ticket.Crear(Codigo, Titulo, Texto(longitud), Categoria.Software, Prioridad.Media, SolicitanteId, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("      ")]
    [InlineData("\tWi-Fi")]
    [InlineData("Wi-Fi ")]
    public void Crear_TituloEnBlancoOConEspaciosEnLosExtremos_LanzaInvarianteViolada(string? titulo)
    {
        var ex = Assert.Throws<InvarianteVioladaException>(() =>
            Ticket.Crear(Codigo, titulo!, Descripcion, Categoria.Software, Prioridad.Media, SolicitanteId, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("            ")]
    [InlineData("\tSin acceso a Wi-Fi")]
    [InlineData("Sin acceso a Wi-Fi ")]
    public void Crear_DescripcionEnBlancoOConEspaciosEnLosExtremos_LanzaInvarianteViolada(string? descripcion)
    {
        var ex = Assert.Throws<InvarianteVioladaException>(() =>
            Ticket.Crear(Codigo, Titulo, descripcion!, Categoria.Software, Prioridad.Media, SolicitanteId, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
    }

    [Fact]
    [Trait("RN", "RN-01")]
    public void Crear_DescripcionConSaltoDeLineaEnMedio_SeAcepta()
    {
        const string descripcion = "Primera línea del problema\nSegunda línea del problema";

        var ticket = Ticket.Crear(Codigo, Titulo, descripcion, Categoria.Software, Prioridad.Media, SolicitanteId, Ahora);

        Assert.Equal(descripcion, ticket.Descripcion);
    }

    [Fact]
    [Trait("RN", "RN-02")]
    public void Crear_PrioridadNoDefinida_LanzaInvarianteViolada()
    {
        var ex = Assert.Throws<InvarianteVioladaException>(() =>
            Ticket.Crear(Codigo, Titulo, Descripcion, Categoria.Software, (Prioridad)7, SolicitanteId, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
    }

    [Fact]
    [Trait("RN", "RN-02")]
    public void Crear_CategoriaNoDefinida_LanzaInvarianteViolada()
    {
        var ex = Assert.Throws<InvarianteVioladaException>(() =>
            Ticket.Crear(Codigo, Titulo, Descripcion, (Categoria)9, Prioridad.Media, SolicitanteId, Ahora));

        Assert.Equal("ERROR_INTERNO", ex.Codigo);
    }
}
