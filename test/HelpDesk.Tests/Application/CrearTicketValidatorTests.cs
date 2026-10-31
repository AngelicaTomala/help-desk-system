using HelpDesk.Application.Tickets.Crear;
using HelpDesk.Domain.Entidades;

namespace HelpDesk.Tests.Application;

/// <summary>
/// Valores frontera de RNF-05 para crear. El validador recibe los textos ya recortados por el handler.
/// </summary>
public class CrearTicketValidatorTests
{
    private static readonly CrearTicketComando Valido =
        new("No abre el sistema", "El sistema de nómina no abre al iniciar sesión.", "Software", "Alta");

    private readonly CrearTicketValidator _validador = new();

    [Theory]
    [Trait("RN", "RN-01")]
    [Trait("CA", "CA-01.3")]
    [InlineData(Ticket.TituloLongitudMinima)]
    [InlineData(Ticket.TituloLongitudMaxima)]
    public void Validar_TituloEnLimite_EsValido(int longitud)
    {
        var resultado = _validador.Validate(Valido with { Titulo = Texto(longitud) });

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [Trait("CA", "CA-01.3")]
    [InlineData(Ticket.TituloLongitudMinima - 1)]
    [InlineData(Ticket.TituloLongitudMaxima + 1)]
    public void Validar_TituloFueraDeLimite_ErrorEnTitulo(int longitud)
    {
        var resultado = _validador.Validate(Valido with { Titulo = Texto(longitud) });

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(CrearTicketComando.Titulo), error.PropertyName);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [Trait("CA", "CA-01.3")]
    [InlineData(Ticket.DescripcionLongitudMinima)]
    [InlineData(Ticket.DescripcionLongitudMaxima)]
    public void Validar_DescripcionEnLimite_EsValido(int longitud)
    {
        var resultado = _validador.Validate(Valido with { Descripcion = Texto(longitud) });

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [Trait("CA", "CA-01.3")]
    [InlineData(Ticket.DescripcionLongitudMinima - 1)]
    [InlineData(Ticket.DescripcionLongitudMaxima + 1)]
    public void Validar_DescripcionFueraDeLimite_ErrorEnDescripcion(int longitud)
    {
        var resultado = _validador.Validate(Valido with { Descripcion = Texto(longitud) });

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(CrearTicketComando.Descripcion), error.PropertyName);
    }

    [Theory]
    [Trait("RN", "RN-01")]
    [Trait("CA", "CA-01.3")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("     ")]
    [InlineData("\t\r\n")]
    public void Validar_TextoEnBlancoOAusente_ErrorObligatorio(string? texto)
    {
        var resultado = _validador.Validate(Valido with { Titulo = texto, Descripcion = texto });

        Assert.Equal(2, resultado.Errors.Count);
        Assert.Contains(resultado.Errors, error =>
            error.PropertyName == nameof(CrearTicketComando.Titulo) && error.ErrorMessage == "El título es obligatorio.");
        Assert.Contains(resultado.Errors, error =>
            error.PropertyName == nameof(CrearTicketComando.Descripcion) && error.ErrorMessage == "La descripción es obligatoria.");
    }

    [Theory]
    [Trait("RN", "RN-02")]
    [Trait("CA", "CA-01.4")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Impresoras")]
    [InlineData("1")]
    [InlineData("-1")]
    [InlineData("Red,Software")]
    [InlineData(" Red ")]
    public void Validar_CategoriaNoValida_ErrorEnCategoria(string? categoria)
    {
        var resultado = _validador.Validate(Valido with { Categoria = categoria });

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(CrearTicketComando.Categoria), error.PropertyName);
    }

    [Theory]
    [Trait("RN", "RN-02")]
    [Trait("CA", "CA-01.4")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Urgente")]
    [InlineData("1")]
    [InlineData("Alta,Baja")]
    [InlineData(" Alta ")]
    public void Validar_PrioridadNoValida_ErrorEnPrioridad(string? prioridad)
    {
        var resultado = _validador.Validate(Valido with { Prioridad = prioridad });

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(CrearTicketComando.Prioridad), error.PropertyName);
    }

    [Theory]
    [Trait("RN", "RN-02")]
    [InlineData("software", "alta")]
    [InlineData("SOFTWARE", "ALTA")]
    [InlineData("aCcEsOs", "critica")]
    public void Validar_EnumSinDistinguirMayusculas_EsValido(string categoria, string prioridad)
    {
        var resultado = _validador.Validate(Valido with { Categoria = categoria, Prioridad = prioridad });

        Assert.True(resultado.IsValid);
    }

    private static string Texto(int longitud) => new('a', longitud);
}
