using HelpDesk.Application.Excepciones;
using HelpDesk.Application.Tickets.Crear;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Tests.Application.Dobles;
using Microsoft.Extensions.Time.Testing;

namespace HelpDesk.Tests.Application;

public class CrearTicketHandlerTests
{
    private const int AnaId = 1;
    private const int FabianId = 6;
    private const int JorgeId = 10;

    private static readonly DateTime Ahora = new(2026, 10, 28, 14, 30, 0, DateTimeKind.Utc);

    private static readonly CrearTicketComando Valido =
        new("No puedo instalar el antivirus", "El instalador muestra un error de permisos al iniciar.", "Software", "Alta");

    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(Ahora));
    private readonly TicketRepositorioFalso _tickets = new();
    private readonly UnidadDeTrabajoFalsa _unidadDeTrabajo;
    private readonly ConsultasTicketFalsas _consultas;
    private GeneradorCodigoTicketFalso _generador = new();

    public CrearTicketHandlerTests()
    {
        _unidadDeTrabajo = new UnidadDeTrabajoFalsa(_tickets);
        _consultas = new ConsultasTicketFalsas(_tickets);
    }

    [Fact]
    [Trait("CA", "CA-01.1")]
    [Trait("RN", "RN-03")]
    public async Task ManejarAsync_SolicitanteDatosValidos_DevuelveDetalleAbiertoSinTecnico()
    {
        var detalle = await Handler().ManejarAsync(Valido, CancellationToken.None);

        Assert.Equal(1, detalle.Id);
        Assert.Equal(GeneradorCodigoTicketFalso.Codigo, detalle.Codigo);
        Assert.Equal(Valido.Titulo, detalle.Titulo);
        Assert.Equal(Valido.Descripcion, detalle.Descripcion);
        Assert.Equal(Categoria.Software, detalle.Categoria);
        Assert.Equal(Prioridad.Alta, detalle.Prioridad);
        Assert.Equal(EstadoTicket.Abierto, detalle.Estado);
        Assert.Equal(new(AnaId, "Ana Solicitante"), detalle.Solicitante);
        Assert.Null(detalle.Tecnico);
        Assert.Equal(Ahora, detalle.FechaCreacion);
        Assert.Equal(Ahora, detalle.FechaActualizacion);
        Assert.Null(detalle.FechaResolucion);
        Assert.Null(detalle.FechaCierre);
        Assert.Equal(EstadoSla.EnTiempo, detalle.EstadoSla);
        Assert.Empty(detalle.Comentarios);
        Assert.Single(_tickets.Agregados);
        Assert.Equal(1, _unidadDeTrabajo.VecesGuardado);
    }

    [Theory]
    [Trait("CA", "CA-01.2")]
    [Trait("CA", "CA-05.1")]
    [Trait("RN", "RN-10")]
    [InlineData("Critica", 2)]
    [InlineData("Alta", 4)]
    [InlineData("Media", 8)]
    [InlineData("Baja", 24)]
    public async Task ManejarAsync_ConPrioridad_VencimientoEsCreacionMasHorasSla(string prioridad, int horas)
    {
        var detalle = await Handler().ManejarAsync(Valido with { Prioridad = prioridad }, CancellationToken.None);

        Assert.Equal(detalle.FechaCreacion.AddHours(horas), detalle.FechaVencimiento);
    }

    [Theory]
    [Trait("CA", "CA-01.10")]
    [Trait("RN", "RN-03")]
    [InlineData(FabianId, Rol.Tecnico)]
    [InlineData(JorgeId, Rol.Supervisor)]
    public async Task ManejarAsync_RolNoSolicitante_LanzaRolNoAutorizadoSinGuardar(int usuarioId, Rol rol)
    {
        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Handler(usuarioId, rol).ManejarAsync(Valido, CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.RolNoAutorizado, ex.Codigo);
        AfirmarNadaCreado();
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public async Task ManejarAsync_RolNoSolicitanteConDatosInvalidos_LanzaRolNoAutorizado()
    {
        var invalido = new CrearTicketComando("Red", "   ", "Impresoras", "1");

        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Handler(FabianId, Rol.Tecnico).ManejarAsync(invalido, CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.RolNoAutorizado, ex.Codigo);
    }

    [Fact]
    [Trait("CA", "CA-01.3")]
    [Trait("CA", "CA-01.4")]
    [Trait("RN", "RN-01")]
    [Trait("RN", "RN-02")]
    public async Task ManejarAsync_DatosInvalidos_LanzaValidacionFallidaConErroresPorCampo()
    {
        // Ejemplo de 03-contrato-api.md §4.1, con una prioridad numérica (decisión 58).
        var invalido = new CrearTicketComando("Red", "     ", "Impresoras", "1");

        var ex = await Assert.ThrowsAsync<SolicitudInvalidaException>(() =>
            Handler().ManejarAsync(invalido, CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.ValidacionFallida, ex.Codigo);
        Assert.Equal(["categoria", "descripcion", "prioridad", "titulo"], ex.Errores.Keys.Order());
        Assert.Equal(["La descripción es obligatoria."], ex.Errores["descripcion"]);
        AfirmarNadaCreado();
    }

    [Fact]
    [Trait("RN", "RN-01")]
    public async Task ManejarAsync_TextosConEspacios_GuardaTextosRecortados()
    {
        var comando = Valido with
        {
            Titulo = "  \tNo puedo instalar el antivirus \n",
            Descripcion = " El instalador muestra   un error.\r\n",
        };

        var detalle = await Handler().ManejarAsync(comando, CancellationToken.None);

        Assert.Equal("No puedo instalar el antivirus", detalle.Titulo);
        Assert.Equal("El instalador muestra   un error.", detalle.Descripcion);
        Assert.Equal("No puedo instalar el antivirus", _tickets.Agregados.Single().Titulo);
    }

    [Fact]
    [Trait("CA", "CA-01.3")]
    [Trait("RN", "RN-01")]
    public async Task ManejarAsync_TituloCortoTrasRecortar_LanzaValidacionFallida()
    {
        // 6 caracteres antes de recortar, 4 después (decisión 31).
        var ex = await Assert.ThrowsAsync<SolicitudInvalidaException>(() =>
            Handler().ManejarAsync(Valido with { Titulo = " abcd " }, CancellationToken.None));

        Assert.Equal(["titulo"], ex.Errores.Keys);
        AfirmarNadaCreado();
    }

    [Fact]
    [Trait("RN", "RN-02")]
    public async Task ManejarAsync_EnumEnMinusculas_GuardaValorCanonico()
    {
        var detalle = await Handler().ManejarAsync(Valido with { Categoria = "red", Prioridad = "CRITICA" }, CancellationToken.None);

        Assert.Equal(Categoria.Red, detalle.Categoria);
        Assert.Equal(Prioridad.Critica, detalle.Prioridad);
    }

    [Fact]
    [Trait("RN", "RN-10")]
    public async Task ManejarAsync_SolicitanteDatosValidos_UsaLaMismaHoraEnFechasYEstadoSla()
    {
        // Cada lectura del reloj lo adelanta: una segunda lectura daría otra hora (RNF-08).
        _reloj.AutoAdvanceAmount = TimeSpan.FromMinutes(1);

        var detalle = await Handler().ManejarAsync(Valido, CancellationToken.None);

        Assert.Equal(Ahora, detalle.FechaCreacion);
        Assert.Equal(Ahora, detalle.FechaActualizacion);
        Assert.Equal(Ahora, _consultas.AhoraRecibido);
        Assert.Equal(DateTimeKind.Utc, detalle.FechaCreacion.Kind);
    }

    [Fact]
    [Trait("RN", "RN-03")]
    public async Task ManejarAsync_GeneradorFalla_PropagaSinGuardar()
    {
        var fallo = new InvalidOperationException("La SEQUENCE CodigoTicket llegó a su máximo.");
        _generador = new GeneradorCodigoTicketFalso { Fallo = fallo };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            Handler().ManejarAsync(Valido, CancellationToken.None));

        Assert.Same(fallo, ex);
        Assert.Empty(_tickets.Agregados);
        Assert.Equal(0, _unidadDeTrabajo.VecesGuardado);
    }

    private CrearTicketHandler Handler(int usuarioId = AnaId, Rol rol = Rol.Solicitante) =>
        new(
            new UsuarioActualFalso(usuarioId, rol),
            new CrearTicketValidator(),
            _generador,
            _tickets,
            _unidadDeTrabajo,
            _consultas,
            _reloj);

    private void AfirmarNadaCreado()
    {
        Assert.Equal(0, _generador.VecesInvocado);
        Assert.Empty(_tickets.Agregados);
        Assert.Equal(0, _unidadDeTrabajo.VecesGuardado);
    }
}
