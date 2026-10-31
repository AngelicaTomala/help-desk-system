using HelpDesk.Application.Tickets.Reclasificar;

namespace HelpDesk.Tests.Application;

/// <summary>
/// Valores de entrada de la categoría al reclasificar (RNF-05, decisiones 58 y 96).
/// </summary>
public class ReclasificarTicketValidatorTests
{
    private readonly ReclasificarTicketValidator _validador = new();

    [Theory]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-02")]
    [InlineData("Hardware")]
    [InlineData("Software")]
    [InlineData("Red")]
    [InlineData("Accesos")]
    [InlineData("Otros")]
    [InlineData("otros")]
    [InlineData("HARDWARE")]
    public void Validar_CategoriaValida_EsValido(string categoria)
    {
        var resultado = _validador.Validate(new ReclasificarTicketComando(12, categoria));

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [Trait("CA", "CA-01.9")]
    [Trait("RN", "RN-02")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Impresoras")]
    [InlineData("1")]
    [InlineData("Red,Software")]
    [InlineData(" Red ")]
    public void Validar_CategoriaInvalida_ErrorEnCategoria(string? categoria)
    {
        var resultado = _validador.Validate(new ReclasificarTicketComando(12, categoria));

        var error = Assert.Single(resultado.Errors);
        Assert.Equal(nameof(ReclasificarTicketComando.Categoria), error.PropertyName);
    }
}
