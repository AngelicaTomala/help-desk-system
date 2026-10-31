using HelpDesk.Application.Excepciones;
using HelpDesk.Application.Tickets.Listar;

namespace HelpDesk.Tests.Application;

/// <summary>
/// Parámetros de la URL del listado como texto (RNF-05, RN-05, decisiones 58 y 101 a 105). Las claves se comprueban ya
/// en camelCase, como las recibe <c>errors</c> (decisión 61).
/// </summary>
public class ListarTicketsValidatorTests
{
    private readonly ListarTicketsValidator _validador = new();

    [Fact]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    public void Validar_SinParametros_EsValido()
    {
        Assert.True(_validador.Validate(new ListarTicketsConsulta()).IsValid);
    }

    [Fact]
    [Trait("RN", "RN-05")]
    public void Validar_ParametrosConListaVacia_EquivalenAAusentes()
    {
        var consulta = new ListarTicketsConsulta([], [], [], [], [], []);

        Assert.True(_validador.Validate(consulta).IsValid);
    }

    [Theory]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("2147483647")]
    public void Validar_PageValido_EsValido(string page)
    {
        Assert.True(_validador.Validate(new ListarTicketsConsulta(Page: [page])).IsValid);
    }

    [Theory]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("01")]
    [InlineData("+1")]
    [InlineData(" 1")]
    [InlineData("2.0")]
    [InlineData("abc")]
    [InlineData("2147483648")]
    public void Validar_PageInvalido_ErrorEnPage(string page)
    {
        AfirmarUnicoError(new ListarTicketsConsulta(Page: [page]), "page");
    }

    [Theory]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    [InlineData("1")]
    [InlineData("20")]
    [InlineData("100")]
    public void Validar_PageSizeValido_EsValido(string pageSize)
    {
        Assert.True(_validador.Validate(new ListarTicketsConsulta(PageSize: [pageSize])).IsValid);
    }

    [Theory]
    [Trait("CA", "CA-02.4")]
    [Trait("RN", "RN-05")]
    [InlineData("0")]
    [InlineData("101")]
    [InlineData("-5")]
    [InlineData("020")]
    [InlineData("1e2")]
    public void Validar_PageSizeInvalido_ErrorEnPageSize(string pageSize)
    {
        AfirmarUnicoError(new ListarTicketsConsulta(PageSize: [pageSize]), "pageSize");
    }

    [Theory]
    [InlineData("Abierto")]
    [InlineData("Asignado")]
    [InlineData("EnProgreso")]
    [InlineData("Resuelto")]
    [InlineData("Cerrado")]
    [InlineData("enprogreso")]
    [InlineData("CERRADO")]
    public void Validar_EstadoValido_EsValido(string estado)
    {
        Assert.True(_validador.Validate(new ListarTicketsConsulta(Estado: [estado])).IsValid);
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [InlineData("Pendiente")]
    [InlineData("En progreso")]
    [InlineData("1")]
    [InlineData(" Abierto")]
    [InlineData("Abierto,Cerrado")]
    public void Validar_EstadoInvalido_ErrorEnEstado(string estado)
    {
        AfirmarUnicoError(new ListarTicketsConsulta(Estado: [estado]), "estado");
    }

    [Theory]
    [Trait("RN", "RN-02")]
    [InlineData("Critica")]
    [InlineData("Alta")]
    [InlineData("Media")]
    [InlineData("Baja")]
    [InlineData("critica")]
    public void Validar_PrioridadValida_EsValido(string prioridad)
    {
        Assert.True(_validador.Validate(new ListarTicketsConsulta(Prioridad: [prioridad])).IsValid);
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [Trait("RN", "RN-02")]
    [InlineData("Crítica")]
    [InlineData("Urgente")]
    [InlineData("0")]
    public void Validar_PrioridadInvalida_ErrorEnPrioridad(string prioridad)
    {
        AfirmarUnicoError(new ListarTicketsConsulta(Prioridad: [prioridad]), "prioridad");
    }

    [Theory]
    [InlineData("6")]
    [InlineData("10")]
    public void Validar_TecnicoIdValido_EsValido(string tecnicoId)
    {
        Assert.True(_validador.Validate(new ListarTicketsConsulta(TecnicoId: [tecnicoId])).IsValid);
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [InlineData("0")]
    [InlineData("06")]
    [InlineData("+6")]
    [InlineData("6.0")]
    [InlineData("abc")]
    public void Validar_TecnicoIdInvalido_ErrorEnTecnicoId(string tecnicoId)
    {
        AfirmarUnicoError(new ListarTicketsConsulta(TecnicoId: [tecnicoId]), "tecnicoId");
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("TRUE")]
    [InlineData("False")]
    public void Validar_SoloVencidosValido_EsValido(string soloVencidos)
    {
        Assert.True(_validador.Validate(new ListarTicketsConsulta(SoloVencidos: [soloVencidos])).IsValid);
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("si")]
    [InlineData("yes")]
    [InlineData(" true")]
    public void Validar_SoloVencidosInvalido_ErrorEnSoloVencidos(string soloVencidos)
    {
        AfirmarUnicoError(new ListarTicketsConsulta(SoloVencidos: [soloVencidos]), "soloVencidos");
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [Trait("RN", "RN-05")]
    [InlineData("estado", "Abierto")]
    [InlineData("prioridad", "Alta")]
    [InlineData("tecnicoId", "6")]
    [InlineData("soloVencidos", "true")]
    [InlineData("page", "1")]
    [InlineData("pageSize", "20")]
    public void Validar_ParametroRepetidoConValoresValidos_ErrorConSuClave(string parametro, string valor)
    {
        AfirmarUnicoError(Consulta(parametro, valor, valor), parametro);
    }

    [Theory]
    [Trait("CA", "CA-02.8")]
    [Trait("RN", "RN-05")]
    [InlineData("estado")]
    [InlineData("prioridad")]
    [InlineData("tecnicoId")]
    [InlineData("soloVencidos")]
    [InlineData("page")]
    [InlineData("pageSize")]
    public void Validar_ParametroVacio_ErrorConSuClave(string parametro)
    {
        AfirmarUnicoError(Consulta(parametro, ""), parametro);
    }

    [Fact]
    [Trait("CA", "CA-02.8")]
    [Trait("RN", "RN-05")]
    public void Validar_VariosParametrosInvalidos_DevuelveTodasLasClaves()
    {
        var consulta = new ListarTicketsConsulta(Estado: ["x"], Prioridad: ["", ""], Page: ["0"], PageSize: ["101"]);

        var errores = Errores(consulta);

        Assert.Equal(["estado", "page", "pageSize", "prioridad"], errores.Keys.Order());
        Assert.All(errores.Values, mensajes => Assert.Single(mensajes));
    }

    private static ListarTicketsConsulta Consulta(string parametro, params string[] valores) =>
        parametro switch
        {
            "estado" => new(Estado: valores),
            "prioridad" => new(Prioridad: valores),
            "tecnicoId" => new(TecnicoId: valores),
            "soloVencidos" => new(SoloVencidos: valores),
            "page" => new(Page: valores),
            "pageSize" => new(PageSize: valores),
            _ => throw new ArgumentOutOfRangeException(nameof(parametro)),
        };

    private IReadOnlyDictionary<string, string[]> Errores(ListarTicketsConsulta consulta) =>
        SolicitudInvalidaException.DesdeValidacion(_validador.Validate(consulta)).Errores;

    private void AfirmarUnicoError(ListarTicketsConsulta consulta, string clave)
    {
        var error = Assert.Single(Errores(consulta));
        Assert.Equal(clave, error.Key);
        Assert.Single(error.Value);
    }
}
