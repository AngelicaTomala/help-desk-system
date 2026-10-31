using HelpDesk.Application.Identificadores;

namespace HelpDesk.Tests.Application;

/// <summary>
/// Formato de un Id en <c>X-Usuario-Id</c> y en <c>{id}</c> (decisión 95).
/// </summary>
public class FormatoIdTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData("10", 10)]
    [InlineData("2147483647", int.MaxValue)]
    public void TryLeer_IdValido_DevuelveElEntero(string texto, int esperado)
    {
        Assert.True(FormatoId.TryLeer(texto, out var id));
        Assert.Equal(esperado, id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("00")]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData(" 1")]
    [InlineData("1 ")]
    [InlineData("01")]
    [InlineData("1,2")]
    [InlineData("1.0")]
    [InlineData("2147483648")]
    [InlineData("99999999999")]
    [InlineData("١")] // Dígito arábigo-índico: no es ASCII.
    [InlineData("１")] // Dígito de ancho completo.
    public void TryLeer_FormatoInvalido_DevuelveFalse(string? texto)
    {
        Assert.False(FormatoId.TryLeer(texto, out _));
    }
}
