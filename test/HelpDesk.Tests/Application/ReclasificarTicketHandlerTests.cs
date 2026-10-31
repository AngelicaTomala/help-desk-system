using HelpDesk.Application.Excepciones;
using HelpDesk.Application.Tickets.Reclasificar;
using HelpDesk.Domain.Entidades;
using HelpDesk.Domain.Enumeraciones;
using HelpDesk.Domain.Excepciones;
using HelpDesk.Tests.Application.Dobles;
using Microsoft.Extensions.Time.Testing;

namespace HelpDesk.Tests.Application;

public class ReclasificarTicketHandlerTests
{
    private const int TicketId = 12;
    private const int AnaId = 1;
    private const int FabianId = 6;
    private const int GabrielaId = 7;
    private const int JorgeId = 10;

    private static readonly DateTime Creacion = new(2026, 10, 28, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Ahora = new(2026, 10, 28, 14, 30, 0, DateTimeKind.Utc);

    private readonly FakeTimeProvider _reloj = new(new DateTimeOffset(Ahora));
    private readonly TicketRepositorioFalso _tickets = new();
    private readonly UnidadDeTrabajoFalsa _unidadDeTrabajo;
    private readonly ConsultasTicketFalsas _consultas;

    public ReclasificarTicketHandlerTests()
    {
        _unidadDeTrabajo = new UnidadDeTrabajoFalsa(_tickets);
        _consultas = new ConsultasTicketFalsas(_tickets);
    }

    [Fact]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_TecnicoAsignado_ActualizaCategoriaYDevuelveDetalle()
    {
        var ticket = Sembrar(EstadoTicket.Asignado);

        var detalle = await Handler(FabianId, Rol.Tecnico).ManejarAsync(Comando("Otros"), CancellationToken.None);

        Assert.Equal(TicketId, detalle.Id);
        Assert.Equal(Categoria.Otros, detalle.Categoria);
        Assert.Equal(EstadoTicket.Asignado, detalle.Estado);
        Assert.Equal(new(FabianId, "Fabián Técnico"), detalle.Tecnico);
        Assert.Equal(Ahora, detalle.FechaActualizacion);
        Assert.Equal(EstadoSla.EnTiempo, detalle.EstadoSla);
        Assert.Equal(Categoria.Otros, ticket.Categoria);
        Assert.Equal(1, _unidadDeTrabajo.VecesGuardado);
    }

    [Theory]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-13")]
    [InlineData(EstadoTicket.Abierto)]
    [InlineData(EstadoTicket.Asignado)]
    [InlineData(EstadoTicket.EnProgreso)]
    public async Task ManejarAsync_SupervisorTicketModificable_ActualizaCategoria(EstadoTicket estado)
    {
        var ticket = Sembrar(estado);

        var detalle = await Handler(JorgeId, Rol.Supervisor).ManejarAsync(Comando("Red"), CancellationToken.None);

        Assert.Equal(Categoria.Red, detalle.Categoria);
        Assert.Equal(estado, ticket.Estado);
        Assert.Equal(Ahora, ticket.FechaActualizacion);
        Assert.Equal(1, _unidadDeTrabajo.VecesGuardado);
    }

    [Fact]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_TecnicoAsignadoTicketEnProgreso_ActualizaCategoria()
    {
        var ticket = Sembrar(EstadoTicket.EnProgreso);

        await Handler(FabianId, Rol.Tecnico).ManejarAsync(Comando("Accesos"), CancellationToken.None);

        Assert.Equal(Categoria.Accesos, ticket.Categoria);
        Assert.Equal(EstadoTicket.EnProgreso, ticket.Estado);
    }

    [Fact]
    [Trait("RN", "RN-02")]
    public async Task ManejarAsync_CategoriaEnMinusculas_GuardaValorCanonico()
    {
        Sembrar(EstadoTicket.Asignado);

        var detalle = await Handler(JorgeId, Rol.Supervisor).ManejarAsync(Comando("aCCesos"), CancellationToken.None);

        Assert.Equal(Categoria.Accesos, detalle.Categoria);
    }

    [Fact]
    [Trait("CA", "CA-01.6")]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_MismaCategoria_NoCambiaFechaActualizacion()
    {
        // Decisión 67: 200 sin cambios; el handler guarda igual y no hay nada que persistir.
        var ticket = Sembrar(EstadoTicket.Asignado);
        var antes = ticket.FechaActualizacion;

        var detalle = await Handler(FabianId, Rol.Tecnico).ManejarAsync(Comando("Hardware"), CancellationToken.None);

        Assert.Equal(Categoria.Hardware, detalle.Categoria);
        Assert.Equal(antes, detalle.FechaActualizacion);
        Assert.Equal(antes, ticket.FechaActualizacion);
        Assert.NotEqual(Ahora, ticket.FechaActualizacion);
    }

    [Fact]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_TecnicoAsignado_UsaLaMismaHoraEnFechaYEstadoSla()
    {
        // Cada lectura del reloj lo adelanta: una segunda lectura daría otra hora (RNF-08).
        Sembrar(EstadoTicket.Asignado);
        _reloj.AutoAdvanceAmount = TimeSpan.FromMinutes(1);

        var detalle = await Handler(FabianId, Rol.Tecnico).ManejarAsync(Comando("Otros"), CancellationToken.None);

        Assert.Equal(Ahora, detalle.FechaActualizacion);
        Assert.Equal(Ahora, _consultas.AhoraRecibido);
        Assert.Equal(DateTimeKind.Utc, detalle.FechaActualizacion.Kind);
    }

    [Fact]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_TicketInexistente_LanzaTicketNoEncontradoSinGuardar()
    {
        Sembrar(EstadoTicket.Asignado);

        var ex = await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            Handler(JorgeId, Rol.Supervisor).ManejarAsync(new ReclasificarTicketComando(99, "Otros"), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.TicketNoEncontrado, ex.Codigo);
        Assert.Equal(0, _unidadDeTrabajo.VecesGuardado);
    }

    [Fact]
    [Trait("CA", "CA-01.9")]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_SupervisorTicketInexistenteCategoriaInvalida_LanzaTicketNoEncontrado()
    {
        var ex = await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            Handler(JorgeId, Rol.Supervisor).ManejarAsync(new ReclasificarTicketComando(99, "Impresoras"), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.TicketNoEncontrado, ex.Codigo);
        Assert.Equal(0, _unidadDeTrabajo.VecesGuardado);
    }

    [Fact]
    [Trait("CA", "CA-01.8")]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_SolicitanteDueno_LanzaRolNoAutorizadoSinGuardar()
    {
        var ticket = Sembrar(EstadoTicket.Asignado);

        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Handler(AnaId, Rol.Solicitante).ManejarAsync(Comando("Otros"), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.RolNoAutorizado, ex.Codigo);
        AfirmarSinCambios(ticket, Categoria.Hardware);
    }

    [Fact]
    [Trait("CA", "CA-01.8")]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_TecnicoNoAsignado_LanzaTicketAjenoSinGuardar()
    {
        var ticket = Sembrar(EstadoTicket.Asignado);

        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Handler(GabrielaId, Rol.Tecnico).ManejarAsync(Comando("Otros"), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.TicketAjeno, ex.Codigo);
        AfirmarSinCambios(ticket, Categoria.Hardware);
    }

    [Fact]
    [Trait("CA", "CA-01.8")]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_TecnicoTicketAbiertoSinTecnico_LanzaTicketAjeno()
    {
        var ticket = Sembrar(EstadoTicket.Abierto);

        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Handler(FabianId, Rol.Tecnico).ManejarAsync(Comando("Otros"), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.TicketAjeno, ex.Codigo);
        AfirmarSinCambios(ticket, Categoria.Hardware);
    }

    [Fact]
    [Trait("CA", "CA-01.8")]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_TecnicoNoAsignadoTicketCerrado_LanzaTicketAjenoNo409()
    {
        var ticket = Sembrar(EstadoTicket.Cerrado);

        var ex = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            Handler(GabrielaId, Rol.Tecnico).ManejarAsync(Comando("Otros"), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.TicketAjeno, ex.Codigo);
        AfirmarSinCambios(ticket, Categoria.Hardware);
    }

    [Theory]
    [Trait("CA", "CA-01.9")]
    [Trait("RN", "RN-02")]
    [InlineData(null)]
    [InlineData("Impresoras")]
    [InlineData("3")]
    public async Task ManejarAsync_CategoriaInvalida_LanzaValidacionFallidaSinCambiar(string? categoria)
    {
        var ticket = Sembrar(EstadoTicket.Asignado);

        var ex = await Assert.ThrowsAsync<SolicitudInvalidaException>(() =>
            Handler(FabianId, Rol.Tecnico).ManejarAsync(Comando(categoria), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.ValidacionFallida, ex.Codigo);
        Assert.Equal(["categoria"], ex.Errores.Keys);
        AfirmarSinCambios(ticket, Categoria.Hardware);
    }

    [Fact]
    [Trait("CA", "CA-01.9")]
    [Trait("RN", "RN-13")]
    public async Task ManejarAsync_SupervisorTicketCerradoCategoriaInvalida_LanzaValidacionFallidaNo409()
    {
        var ticket = Sembrar(EstadoTicket.Cerrado);

        var ex = await Assert.ThrowsAsync<SolicitudInvalidaException>(() =>
            Handler(JorgeId, Rol.Supervisor).ManejarAsync(Comando("Impresoras"), CancellationToken.None));

        Assert.Equal(CodigosErrorAplicacion.ValidacionFallida, ex.Codigo);
        AfirmarSinCambios(ticket, Categoria.Hardware);
    }

    [Theory]
    [Trait("CA", "CA-01.7")]
    [Trait("RN", "RN-13")]
    [InlineData(EstadoTicket.Resuelto, "Otros")]
    [InlineData(EstadoTicket.Resuelto, "Hardware")]
    [InlineData(EstadoTicket.Cerrado, "Otros")]
    [InlineData(EstadoTicket.Cerrado, "Hardware")]
    public async Task ManejarAsync_TicketNoModificable_LanzaTicketNoModificableSinCambiar(EstadoTicket estado, string categoria)
    {
        // Decisión 70: también con la categoría actual (Hardware).
        var ticket = Sembrar(estado);

        var ex = await Assert.ThrowsAsync<TransicionInvalidaException>(() =>
            Handler(JorgeId, Rol.Supervisor).ManejarAsync(Comando(categoria), CancellationToken.None));

        Assert.Equal(CodigosError.TicketNoModificable, ex.Codigo);
        Assert.Equal(estado, ticket.Estado);
        AfirmarSinCambios(ticket, Categoria.Hardware);
    }

    private static ReclasificarTicketComando Comando(string? categoria) => new(TicketId, categoria);

    /// <summary>
    /// Ticket 12 del elenco (Hardware, Media), de Ana y con Fabián como técnico desde <c>Asignado</c>, llevado al estado
    /// pedido con los métodos de Domain antes de la hora de referencia.
    /// </summary>
    private Ticket Sembrar(EstadoTicket estado)
    {
        var ticket = Ticket.Crear(
            "HD-000012", "Monitor sin imagen", "El monitor del puesto 4 no muestra imagen al encender.",
            Categoria.Hardware, Prioridad.Media, AnaId, Creacion);

        if (estado != EstadoTicket.Abierto)
        {
            ticket.Asignar(FabianId, Creacion.AddMinutes(30));
        }

        if (estado is EstadoTicket.EnProgreso or EstadoTicket.Resuelto or EstadoTicket.Cerrado)
        {
            ticket.IniciarAtencion(Creacion.AddHours(1));
        }

        if (estado is EstadoTicket.Resuelto or EstadoTicket.Cerrado)
        {
            ticket.Resolver("Se cambió el cable de video.", FabianId, Creacion.AddHours(2));
        }

        if (estado == EstadoTicket.Cerrado)
        {
            ticket.Cerrar(Creacion.AddHours(3));
        }

        return _tickets.Sembrar(ticket, TicketId);
    }

    private ReclasificarTicketHandler Handler(int usuarioId, Rol rol) =>
        new(
            new UsuarioActualFalso(usuarioId, rol),
            new ReclasificarTicketValidator(),
            _tickets,
            _unidadDeTrabajo,
            _consultas,
            _reloj);

    private void AfirmarSinCambios(Ticket ticket, Categoria categoriaOriginal)
    {
        Assert.Equal(categoriaOriginal, ticket.Categoria);
        Assert.NotEqual(Ahora, ticket.FechaActualizacion);
        Assert.Equal(0, _unidadDeTrabajo.VecesGuardado);
    }
}
