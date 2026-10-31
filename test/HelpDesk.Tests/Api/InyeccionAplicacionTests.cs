using FluentValidation;
using HelpDesk.Api.Configuracion;
using HelpDesk.Api.Identidad;
using HelpDesk.Application.Abstracciones;
using HelpDesk.Application.Tickets.Crear;
using HelpDesk.Application.Tickets.Listar;
using HelpDesk.Application.Tickets.Obtener;
using HelpDesk.Application.Tickets.Reclasificar;
using HelpDesk.Domain.Enumeraciones;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Tests.Api;

/// <summary>
/// Registro de los casos de uso de Application, de <see cref="TimeProvider"/> y del usuario actual.
/// </summary>
public class InyeccionAplicacionTests(HelpDeskApiFactory factory) : IClassFixture<HelpDeskApiFactory>
{
    [Fact]
    public void AgregarAplicacion_TimeProvider_RegistraElRelojDelSistemaSingleton()
    {
        var services = new ServiceCollection().AgregarAplicacion();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(TimeProvider));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Same(TimeProvider.System, descriptor.ImplementationInstance);
    }

    [Theory]
    [InlineData(typeof(IValidator<CrearTicketComando>), typeof(CrearTicketValidator))]
    [InlineData(typeof(IValidator<ReclasificarTicketComando>), typeof(ReclasificarTicketValidator))]
    [InlineData(typeof(CrearTicketHandler), typeof(CrearTicketHandler))]
    [InlineData(typeof(ReclasificarTicketHandler), typeof(ReclasificarTicketHandler))]
    [InlineData(typeof(ObtenerTicketHandler), typeof(ObtenerTicketHandler))]
    [InlineData(typeof(IValidator<ListarTicketsConsulta>), typeof(ListarTicketsValidator))]
    [InlineData(typeof(ListarTicketsHandler), typeof(ListarTicketsHandler))]
    public void AgregarAplicacion_CasosDeUso_RegistraScoped(Type servicio, Type implementacion)
    {
        var services = new ServiceCollection().AgregarAplicacion();

        var descriptor = Assert.Single(services, d => d.ServiceType == servicio);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(implementacion, descriptor.ImplementationType);
    }

    [Fact]
    public void AgregarAplicacion_UsuarioActual_EsLaMismaInstanciaEnLaPeticion()
    {
        using var provider = new ServiceCollection().AgregarAplicacion().BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        Assert.Same(
            scope.ServiceProvider.GetRequiredService<UsuarioActual>(),
            scope.ServiceProvider.GetRequiredService<IUsuarioActual>());
    }

    [Theory]
    [InlineData(typeof(CrearTicketHandler))]
    [InlineData(typeof(ReclasificarTicketHandler))]
    [InlineData(typeof(ObtenerTicketHandler))]
    [InlineData(typeof(ListarTicketsHandler))]
    public void ResolverHandler_ConLaApi_ResuelveSusDependencias(Type handler)
    {
        using var scope = factory.Services.CreateScope();

        Assert.IsType(handler, scope.ServiceProvider.GetRequiredService(handler));
    }

    [Fact]
    public void UsuarioActual_SinFijar_LanzaInvalidOperationException()
    {
        var usuario = new UsuarioActual();

        Assert.Throws<InvalidOperationException>(() => usuario.Id);
        Assert.Throws<InvalidOperationException>(() => usuario.Rol);
    }

    [Fact]
    public void UsuarioActual_Fijado_DevuelveIdYRol()
    {
        var usuario = new UsuarioActual();

        usuario.Fijar(10, Rol.Supervisor);

        Assert.Equal(10, usuario.Id);
        Assert.Equal(Rol.Supervisor, usuario.Rol);
    }

    [Fact]
    public void UsuarioActual_FijadoDosVeces_LanzaInvalidOperationException()
    {
        var usuario = new UsuarioActual();
        usuario.Fijar(1, Rol.Solicitante);

        Assert.Throws<InvalidOperationException>(() => usuario.Fijar(10, Rol.Supervisor));
    }
}
